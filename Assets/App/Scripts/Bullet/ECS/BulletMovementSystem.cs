using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

namespace ZombiesWar.Bullet.ECS
{
    [BurstCompile]
    public partial struct BulletMovementJob : IJobEntity
    {
        public float DeltaTime;

        [ReadOnly] public ComponentLookup<LocalTransform> Transforms;
        [ReadOnly] public ComponentLookup<EnemyHealth> EnemyHealths;

        void Execute(ref BulletData data, ref BulletLifeData life, ref LocalTransform transform)
        {
            if (life.HasHit)
                return;

            if (data.MovementType != BulletMovementType.PhysicsProjectile)
                return;

            life.RemainingLife -= DeltaTime;
            if (life.RemainingLife <= 0f)
            {
                life.HasHit = true;
                return;
            }

            life.PreviousPosition = transform.Position;

            if (Transforms.HasComponent(data.TargetEntity) && EnemyHealths.HasComponent(data.TargetEntity))
            {
                var targetHealth = EnemyHealths[data.TargetEntity];
                if (targetHealth.Value <= 0f)
                {
                    life.HasHit = true;
                    return;
                }

                var targetPos = Transforms[data.TargetEntity].Position;
                var dir = math.normalizesafe(targetPos - transform.Position);
                transform.Position += dir * data.Speed * DeltaTime;
            }
            else
            {
                life.HasHit = true;
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BulletSystemGroup))]
    public partial struct BulletMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformsLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var healthsLookup = SystemAPI.GetComponentLookup<EnemyHealth>(true);

            var job = new BulletMovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Transforms = transformsLookup,
                EnemyHealths = healthsLookup,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
