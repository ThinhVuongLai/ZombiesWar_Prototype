using App.Enemy;

namespace App.Enemy.Attack
{
    public interface IEnemyAttackStrategy
    {
        EnemyAttackType AttackType { get; }
        float Range { get; }
        void Execute(IEnemyView view, IPlayerTargetProvider target, float damage);
    }
}
