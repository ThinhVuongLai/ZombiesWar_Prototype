using Unity.Entities;

public struct EnemyStats : IComponentData
{
    public float MoveSpeed;
    public float AttackDamage;
    public float AttackRange;
    public float DetectionRange;
    public float AttackCooldown;
}
