using Unity.Entities;

public readonly struct EnemyDealtDamageMessage
{
    public readonly float Damage;
    public readonly WeaponType AttackType;

    public EnemyDealtDamageMessage(float damage, WeaponType attackType)
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
