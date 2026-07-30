using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using App.Player.ECS;

namespace ZombiesWar.Bullet.ECS
{
    [BurstCompile]
    public partial struct BulletMovementJob : IJobEntity
    {
        public float DeltaTime;

        [ReadOnly] [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> Transforms;
        [ReadOnly] [NativeDisableContainerSafetyRestriction] public ComponentLookup<EnemyHealth> EnemyHealths;
        [ReadOnly] [NativeDisableContainerSafetyRestriction] public ComponentLookup<PlayerHealth> PlayerHealths;

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

            if (!Transforms.HasComponent(data.TargetEntity))
            {
                life.HasHit = true;
                return;
            }

            if (EnemyHealths.HasComponent(data.TargetEntity))
            {
                var targetHealth = EnemyHealths[data.TargetEntity];
                if (targetHealth.Value <= 0f)
                {
                    life.HasHit = true;
                    return;
                }
            }
            else if (PlayerHealths.HasComponent(data.TargetEntity))
            {
                var playerHealth = PlayerHealths[data.TargetEntity];
                if (playerHealth.Value <= 0f)
                {
                    life.HasHit = true;
                    return;
                }
            }

            var targetPos = Transforms[data.TargetEntity].Position;
            var dir = math.normalizesafe(targetPos - transform.Position);
            transform.Position += dir * data.Speed * DeltaTime;
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
            var playerHealthsLookup = SystemAPI.GetComponentLookup<PlayerHealth>(true);

            var job = new BulletMovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Transforms = transformsLookup,
                EnemyHealths = healthsLookup,
                PlayerHealths = playerHealthsLookup,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
