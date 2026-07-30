using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct EnemyCombatJob : IJobEntity
{
    [Unity.Collections.ReadOnly] public double ElapsedTime;

    void Execute(ref EnemyCombatState state, in EnemyStats stats)
    {
        if (state.DetectionState != EnemyDetectionState.InAttackRange)
            return;

        if (ElapsedTime - state.LastAttackTime >= stats.AttackCooldown)
        {
            state.NeedsCombatResult = true;
            state.LastAttackTime = (float)ElapsedTime;
        }
    }
}

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(EnemySystemGroup))]
[UpdateAfter(typeof(EnemyDetectionSystem))]
public partial struct EnemyCombatSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new EnemyCombatJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}
