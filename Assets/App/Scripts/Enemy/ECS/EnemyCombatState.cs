using Unity.Entities;

public struct EnemyCombatState : IComponentData
{
    public float LastAttackTime;
    public EnemyDetectionState DetectionState;
    public bool NeedsCombatResult;
}
