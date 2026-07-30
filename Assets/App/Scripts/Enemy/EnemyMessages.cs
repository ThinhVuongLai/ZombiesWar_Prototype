using Unity.Entities;

public readonly struct EnemyDealtDamageMessage
{
    public readonly float Damage;
    public readonly EnemyAttackType AttackType;

    public EnemyDealtDamageMessage(float damage, EnemyAttackType attackType)
    {
        Damage = damage;
        AttackType = attackType;
    }
}

public readonly struct EnemyTookDamageMessage
{
    public readonly Entity EnemyEntity;
    public readonly float Damage;

    public EnemyTookDamageMessage(Entity enemyEntity, float damage)
    {
        EnemyEntity = enemyEntity;
        Damage = damage;
    }
}
