using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyDetectionJob : IJobEntity
{
    [Unity.Collections.ReadOnly] public float3 PlayerPosition;
    [Unity.Collections.ReadOnly] public bool PlayerIsAlive;

    void Execute(ref EnemyCombatState state, in EnemyStats stats, in LocalTransform transform)
    {
        if (!PlayerIsAlive)
        {
            state.DetectionState = EnemyDetectionState.None;
            return;
        }

        float sqrDistance = math.distancesq(transform.Position, PlayerPosition);
        float attackRangeSq = stats.AttackRange * stats.AttackRange;
        float detectionRangeSq = stats.DetectionRange * stats.DetectionRange;

        if (sqrDistance <= attackRangeSq)
            state.DetectionState = EnemyDetectionState.InAttackRange;
        else if (sqrDistance <= detectionRangeSq)
            state.DetectionState = EnemyDetectionState.InDetectionRange;
        else
            state.DetectionState = EnemyDetectionState.None;
    }
}

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(EnemySystemGroup))]
public partial struct EnemyDetectionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTargetECSData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerData = SystemAPI.GetSingleton<PlayerTargetECSData>();

        var job = new EnemyDetectionJob
        {
            PlayerPosition = playerData.Position,
            PlayerIsAlive = playerData.IsAlive,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}
