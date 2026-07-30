using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

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

                if (targetEntity == Entity.Null)
                {
                    bulletLifeDatas[i] = new BulletLifeData
                    {
                        RemainingLife = bulletLifeDatas[i].RemainingLife,
                        StartPosition = startPos,
                        PreviousPosition = bulletLifeDatas[i].PreviousPosition,
                        HasHit = true,
                    };
                    continue;
                }

                if (!state.EntityManager.Exists(targetEntity))
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

                if (!state.EntityManager.HasComponent<LocalTransform>(targetEntity) ||
                    !state.EntityManager.HasComponent<EnemyHealth>(targetEntity))
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

                var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(targetEntity);
                var targetHealth = state.EntityManager.GetComponentData<EnemyHealth>(targetEntity);

                if (targetHealth.Value <= 0f)
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

                float rangeSq = bulletDatas[i].Lifetime * bulletDatas[i].Speed;
                rangeSq *= rangeSq;

                float sqrDist = math.distancesq(targetTransform.Position, startPos);

                if (sqrDist <= rangeSq)
                {
                    state.EntityManager.SetComponentData(targetEntity, new EnemyHealth
                    {
                        Value = math.max(targetHealth.Value - bulletDatas[i].Damage, 0f)
                    });
                }

                bulletLifeDatas[i] = new BulletLifeData
                {
                    RemainingLife = 0f,
                    StartPosition = startPos,
                    PreviousPosition = bulletLifeDatas[i].PreviousPosition,
                    HasHit = true,
                };
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
    }
}
