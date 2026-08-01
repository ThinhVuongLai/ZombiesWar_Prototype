using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using App.Player.ECS;

namespace ZombiesWar.Bullet.ECS
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BulletSystem : ISystem
    {
        EntityQuery _bulletQuery;
        EntityQuery _enemyQuery;
        EntityQuery _playerQuery;

        const float HitRadius = 0.5f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletData>();

            _bulletQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<BulletData>(),
                ComponentType.ReadWrite<BulletLifeData>(),
                ComponentType.ReadWrite<LocalTransform>()
            );

            _enemyQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<EnemyHealth>()
            );

            _playerQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<PlayerHealth>()
            );
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var entityManager = state.EntityManager;

            var bulletEntities = _bulletQuery.ToEntityArray(Allocator.Temp);
            var bulletDatas = _bulletQuery.ToComponentDataArray<BulletData>(Allocator.Temp);
            var bulletLifeDatas = _bulletQuery.ToComponentDataArray<BulletLifeData>(Allocator.Temp);
            var bulletTransforms = _bulletQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            var enemyEntities = _enemyQuery.ToEntityArray(Allocator.Temp);
            var enemyTransforms = _enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            var playerEntities = _playerQuery.ToEntityArray(Allocator.Temp);
            var playerTransforms = _playerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                if (bulletLifeDatas[i].HasHit)
                    continue;

                var data = bulletDatas[i];
                var life = bulletLifeDatas[i];
                var transform = bulletTransforms[i];

                if (data.MovementType == BulletMovementType.RayCast)
                {
                    if (TryRayCastHit(entityManager, data.TargetEntity, data.Lifetime * data.Speed, transform.Position))
                        ApplyDamage(entityManager, data.TargetEntity, data.Damage);
                    life.HasHit = true;
                }
                else
                {
                    float3 previousPosition = transform.Position;

                    if (!ProcessMovement(entityManager, ref data, ref life, ref transform, deltaTime))
                    {
                        bulletDatas[i] = data;
                        bulletLifeDatas[i] = life;
                        bulletTransforms[i] = transform;
                        continue;
                    }

                    life.PreviousPosition = previousPosition;

                    if (TrySegmentHit(entityManager, transform.Position, previousPosition, data.Damage,
                            enemyEntities, enemyTransforms,
                            playerEntities, playerTransforms))
                    {
                        life.HasHit = true;
                        bulletDatas[i] = data;
                        bulletLifeDatas[i] = life;
                        bulletTransforms[i] = transform;
                        continue;
                    }
                }

                bulletDatas[i] = data;
                bulletLifeDatas[i] = life;
                bulletTransforms[i] = transform;
            }

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                entityManager.SetComponentData(bulletEntities[i], bulletLifeDatas[i]);
                entityManager.SetComponentData(bulletEntities[i], bulletTransforms[i]);
            }

            CleanupHasHit(bulletEntities, bulletLifeDatas, entityManager);

            bulletEntities.Dispose();
            bulletDatas.Dispose();
            bulletLifeDatas.Dispose();
            bulletTransforms.Dispose();
            enemyEntities.Dispose();
            enemyTransforms.Dispose();
            playerEntities.Dispose();
            playerTransforms.Dispose();
        }

        static bool TryRayCastHit(EntityManager entityManager, Entity target, float range, float3 origin)
        {
            if (target == Entity.Null || !entityManager.Exists(target) || !entityManager.HasComponent<LocalTransform>(target))
                return false;

            var targetPosition = entityManager.GetComponentData<LocalTransform>(target).Position;
            return math.distancesq(targetPosition, origin) <= range * range;
        }

        static bool ProcessMovement(EntityManager entityManager, ref BulletData data, ref BulletLifeData life,
            ref LocalTransform transform, float deltaTime)
        {
            life.RemainingLife -= deltaTime;
            if (life.RemainingLife <= 0f)
            {
                life.HasHit = true;
                return false;
            }

            var target = data.TargetEntity;
            if (target == Entity.Null || !entityManager.Exists(target) || !entityManager.HasComponent<LocalTransform>(target))
            {
                life.HasHit = true;
                return false;
            }

            if (!IsTargetAlive(entityManager, target))
            {
                life.HasHit = true;
                return false;
            }

            var targetPosition = entityManager.GetComponentData<LocalTransform>(target).Position;
            var direction = math.normalizesafe(targetPosition - transform.Position);
            transform.Position += direction * data.Speed * deltaTime;
            return true;
        }

        static bool IsTargetAlive(EntityManager entityManager, Entity target)
        {
            if (entityManager.HasComponent<EnemyHealth>(target))
                return entityManager.GetComponentData<EnemyHealth>(target).Value > 0f;
            if (entityManager.HasComponent<PlayerHealth>(target))
                return entityManager.GetComponentData<PlayerHealth>(target).Value > 0f;
            return true;
        }

        static bool TrySegmentHit(EntityManager entityManager, float3 from, float3 to, float damage,
            NativeArray<Entity> enemyEntities, NativeArray<LocalTransform> enemyTransforms,
            NativeArray<Entity> playerEntities, NativeArray<LocalTransform> playerTransforms)
        {
            float3 segment = from - to;
            float segmentLengthSquared = math.lengthsq(segment);
            if (segmentLengthSquared < 0.0001f)
                return false;

            const float hitRadiusSq = HitRadius * HitRadius;

            for (int j = 0; j < enemyEntities.Length; j++)
            {
                if (!entityManager.HasComponent<EnemyHealth>(enemyEntities[j]))
                    continue;

                var h = entityManager.GetComponentData<EnemyHealth>(enemyEntities[j]);
                if (h.Value <= 0f)
                    continue;

                float t = math.dot(enemyTransforms[j].Position - to, segment) / segmentLengthSquared;
                if (t < 0f || t > 1f)
                    continue;

                float3 closest = to + t * segment;
                if (math.distancesq(enemyTransforms[j].Position, closest) <= hitRadiusSq)
                {
                    h.Value = math.max(h.Value - damage, 0f);
                    entityManager.SetComponentData(enemyEntities[j], h);
                    return true;
                }
            }

            for (int j = 0; j < playerEntities.Length; j++)
            {
                if (!entityManager.HasComponent<PlayerHealth>(playerEntities[j]))
                    continue;

                var h = entityManager.GetComponentData<PlayerHealth>(playerEntities[j]);
                if (h.Value <= 0f)
                    continue;

                float t = math.dot(playerTransforms[j].Position - to, segment) / segmentLengthSquared;
                if (t < 0f || t > 1f)
                    continue;

                float3 closest = to + t * segment;
                if (math.distancesq(playerTransforms[j].Position, closest) <= hitRadiusSq)
                {
                    h.Value = math.max(h.Value - damage, 0f);
                    entityManager.SetComponentData(playerEntities[j], h);
                    return true;
                }
            }

            return false;
        }

        static void ApplyDamage(EntityManager entityManager, Entity target, float damage)
        {
            if (entityManager.HasComponent<EnemyHealth>(target))
            {
                var h = entityManager.GetComponentData<EnemyHealth>(target);
                h.Value = math.max(h.Value - damage, 0f);
                entityManager.SetComponentData(target, h);
            }
            else if (entityManager.HasComponent<PlayerHealth>(target))
            {
                var h = entityManager.GetComponentData<PlayerHealth>(target);
                h.Value = math.max(h.Value - damage, 0f);
                entityManager.SetComponentData(target, h);
            }
        }

        static void CleanupHasHit(NativeArray<Entity> bulletEntities, NativeArray<BulletLifeData> bulletLifeDatas,
            EntityManager entityManager)
        {
            for (int i = 0; i < bulletEntities.Length; i++)
            {
                if (bulletLifeDatas[i].HasHit)
                    entityManager.DestroyEntity(bulletEntities[i]);
            }
        }
    }
}
