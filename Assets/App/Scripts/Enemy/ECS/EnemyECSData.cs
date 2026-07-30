using Unity.Entities;

public enum EnemyDetectionState : byte
{
    None,
    InDetectionRange,
    InAttackRange
}

public enum EnemyAttackType : byte
{
    None = 0,
    Melee = 1,
    Ranged = 2,
}
