using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using App.Player.ECS;

namespace ZombiesWar.Bullet.ECS
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BulletSystemGroup))]
    [UpdateAfter(typeof(BulletMovementSystem))]
    public partial struct BulletHitDetectionSystem : ISystem
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
                ComponentType.ReadOnly<LocalTransform>()
            );

            _enemyQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadWrite<EnemyHealth>()
            );

            _playerQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadWrite<PlayerHealth>()
            );
        }

        public void OnUpdate(ref SystemState state)
        {
            var bulletEntities = _bulletQuery.ToEntityArray(Allocator.Temp);
            var bulletDatas = _bulletQuery.ToComponentDataArray<BulletData>(Allocator.Temp);
            var bulletLifeDatas = _bulletQuery.ToComponentDataArray<BulletLifeData>(Allocator.Temp);
            var bulletTransforms = _bulletQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            var enemyEntities = _enemyQuery.ToEntityArray(Allocator.Temp);
            var enemyTransforms = _enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var enemyHealths = _enemyQuery.ToComponentDataArray<EnemyHealth>(Allocator.Temp);

            var playerEntities = _playerQuery.ToEntityArray(Allocator.Temp);
            var playerTransforms = _playerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var playerHealths = _playerQuery.ToComponentDataArray<PlayerHealth>(Allocator.Temp);

            const float hitRadiusSq = HitRadius * HitRadius;

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                if (bulletLifeDatas[i].HasHit)
                    continue;

                if (bulletDatas[i].MovementType != BulletMovementType.PhysicsProjectile)
                    continue;

                float3 currentPos = bulletTransforms[i].Position;
                float3 prevPos = bulletLifeDatas[i].PreviousPosition;

                float3 movementDir = currentPos - prevPos;
                float segLengthSq = math.lengthsq(movementDir);

                if (segLengthSq < 0.0001f)
                    continue;

                bool hitSomething = false;

                for (int j = 0; j < enemyEntities.Length; j++)
                {
                    if (enemyHealths[j].Value <= 0f)
                        continue;

                    float3 enemyPos = enemyTransforms[j].Position;

                    float t = math.dot(enemyPos - prevPos, movementDir) / segLengthSq;
                    t = math.clamp(t, 0f, 1f);

                    float3 closestPoint = prevPos + t * movementDir;
                    float sqrDist = math.distancesq(enemyPos, closestPoint);

                    if (sqrDist <= hitRadiusSq)
                    {
                        enemyHealths[j] = new EnemyHealth
                        {
                            Value = math.max(enemyHealths[j].Value - bulletDatas[i].Damage, 0f)
                        };

                        bulletLifeDatas[i] = new BulletLifeData
                        {
                            RemainingLife = bulletLifeDatas[i].RemainingLife,
                            StartPosition = bulletLifeDatas[i].StartPosition,
                            PreviousPosition = prevPos,
                            HasHit = true,
                        };
                        hitSomething = true;
                        break;
                    }
                }

                if (!hitSomething)
                {
                    for (int j = 0; j < playerEntities.Length; j++)
                    {
                        if (playerHealths[j].Value <= 0f)
                            continue;

                        float3 playerPos = playerTransforms[j].Position;

                        float t = math.dot(playerPos - prevPos, movementDir) / segLengthSq;
                        t = math.clamp(t, 0f, 1f);

                        float3 closestPoint = prevPos + t * movementDir;
                        float sqrDist = math.distancesq(playerPos, closestPoint);

                        if (sqrDist <= hitRadiusSq)
                        {
                            playerHealths[j] = new PlayerHealth
                            {
                                Value = math.max(playerHealths[j].Value - bulletDatas[i].Damage, 0f),
                                MaxValue = playerHealths[j].MaxValue,
                            };

                            bulletLifeDatas[i] = new BulletLifeData
                            {
                                RemainingLife = bulletLifeDatas[i].RemainingLife,
                                StartPosition = bulletLifeDatas[i].StartPosition,
                                PreviousPosition = prevPos,
                                HasHit = true,
                            };
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                state.EntityManager.SetComponentData(bulletEntities[i], bulletLifeDatas[i]);
            }

            for (int j = 0; j < enemyEntities.Length; j++)
            {
                state.EntityManager.SetComponentData(enemyEntities[j], enemyHealths[j]);
            }

            for (int j = 0; j < playerEntities.Length; j++)
            {
                state.EntityManager.SetComponentData(playerEntities[j], playerHealths[j]);
            }

            bulletEntities.Dispose();
            bulletDatas.Dispose();
            bulletLifeDatas.Dispose();
            bulletTransforms.Dispose();
            enemyEntities.Dispose();
            enemyTransforms.Dispose();
            enemyHealths.Dispose();
            playerEntities.Dispose();
            playerTransforms.Dispose();
            playerHealths.Dispose();
        }
    }
}
