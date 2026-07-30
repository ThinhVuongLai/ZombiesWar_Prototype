using App.Enemy;

namespace App.Enemy.Attack
{
    public interface IEnemyAttackStrategy
    {
        WeaponType AttackType { get; }
        void Execute(IEnemyView view, IPlayerTargetProvider target, float damage);
    }
}
