using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using App.Player.ECS;

namespace ZombiesWar.Bullet.ECS
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BulletSystemGroup))]
    public partial struct BulletRayCastSystem : ISystem
    {
        EntityQuery _bulletQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletData>();

            _bulletQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<BulletData>(),
                ComponentType.ReadWrite<BulletLifeData>(),
                ComponentType.ReadOnly<LocalTransform>()
            );
        }

        public void OnUpdate(ref SystemState state)
        {
            var bulletEntities = _bulletQuery.ToEntityArray(Allocator.Temp);
            var bulletDatas = _bulletQuery.ToComponentDataArray<BulletData>(Allocator.Temp);
            var bulletLifeDatas = _bulletQuery.ToComponentDataArray<BulletLifeData>(Allocator.Temp);
            var bulletTransforms = _bulletQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                if (bulletLifeDatas[i].HasHit)
                    continue;

                if (bulletDatas[i].MovementType != BulletMovementType.RayCast)
                    continue;

                float3 startPos = bulletLifeDatas[i].StartPosition;
                Entity targetEntity = bulletDatas[i].TargetEntity;

                if (targetEntity == Entity.Null || !state.EntityManager.Exists(targetEntity))
                {
                    bulletLifeDatas[i] = new BulletLifeData
                    {
                        RemainingLife = 0f,
                        StartPosition = startPos,
                        PreviousPosition = bulletLifeDatas[i].PreviousPosition,
                        HasHit = true,
                    };
                    continue;
                }

                var bulletData = bulletDatas[i];
                var lifeData = bulletLifeDatas[i];

                if (TryDealDamageToEnemy(state.EntityManager, targetEntity, startPos, bulletData, ref lifeData))
                {
                    bulletLifeDatas[i] = lifeData;
                    continue;
                }

                TryDealDamageToPlayer(state.EntityManager, targetEntity, startPos, bulletData, ref lifeData);
                bulletLifeDatas[i] = lifeData;
            }

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                state.EntityManager.SetComponentData(bulletEntities[i], bulletLifeDatas[i]);
            }

            bulletEntities.Dispose();
            bulletDatas.Dispose();
            bulletLifeDatas.Dispose();
            bulletTransforms.Dispose();
        }

        static bool TryDealDamageToEnemy(EntityManager em, Entity targetEntity, float3 startPos,
            BulletData data, ref BulletLifeData lifeData)
        {
            if (!em.HasComponent<EnemyHealth>(targetEntity))
                return false;

            var targetHealth = em.GetComponentData<EnemyHealth>(targetEntity);
            if (targetHealth.Value <= 0f)
            {
                lifeData = new BulletLifeData
                {
                    RemainingLife = 0f,
                    StartPosition = startPos,
                    PreviousPosition = lifeData.PreviousPosition,
                    HasHit = true,
                };
                return true;
            }

            if (!em.HasComponent<LocalTransform>(targetEntity))
                return false;

            var targetTransform = em.GetComponentData<LocalTransform>(targetEntity);

            float rangeSq = data.Lifetime * data.Speed;
            rangeSq *= rangeSq;

            float sqrDist = math.distancesq(targetTransform.Position, startPos);

            if (sqrDist <= rangeSq)
            {
                em.SetComponentData(targetEntity, new EnemyHealth
                {
                    Value = math.max(targetHealth.Value - data.Damage, 0f)
                });
            }

            lifeData = new BulletLifeData
            {
                RemainingLife = 0f,
                StartPosition = startPos,
                PreviousPosition = lifeData.PreviousPosition,
                HasHit = true,
            };
            return true;
        }

        static void TryDealDamageToPlayer(EntityManager em, Entity targetEntity, float3 startPos,
            BulletData data, ref BulletLifeData lifeData)
        {
            if (!em.HasComponent<PlayerHealth>(targetEntity))
                return;

            var playerHealth = em.GetComponentData<PlayerHealth>(targetEntity);
            if (playerHealth.Value <= 0f)
            {
                lifeData = new BulletLifeData
                {
                    RemainingLife = 0f,
                    StartPosition = startPos,
                    PreviousPosition = lifeData.PreviousPosition,
                    HasHit = true,
                };
                return;
            }

            if (!em.HasComponent<LocalTransform>(targetEntity))
                return;

            var targetTransform = em.GetComponentData<LocalTransform>(targetEntity);

            float rangeSq = data.Lifetime * data.Speed;
            rangeSq *= rangeSq;

            float sqrDist = math.distancesq(targetTransform.Position, startPos);

            if (sqrDist <= rangeSq)
            {
                em.SetComponentData(targetEntity, new PlayerHealth
                {
                    Value = math.max(playerHealth.Value - data.Damage, 0f),
                    MaxValue = playerHealth.MaxValue,
                });
            }

            lifeData = new BulletLifeData
            {
                RemainingLife = 0f,
                StartPosition = startPos,
                PreviousPosition = lifeData.PreviousPosition,
                HasHit = true,
            };
        }
    }
}
