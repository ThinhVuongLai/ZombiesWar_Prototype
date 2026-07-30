using App.Enemy;

namespace App.Enemy.Attack
{
    public interface IEnemyAttackStrategy
    {
        EnemyAttackType AttackType { get; }
        void Execute(IEnemyView view, IPlayerTargetProvider target, float damage);
    }
}
