using System.Collections.Generic;
using App.Enemy;

namespace App.Enemy.Attack
{
    public class AttackStrategyRegistry
    {
        readonly Dictionary<EnemyAttackType, IEnemyAttackStrategy> _strategies;

        public AttackStrategyRegistry()
        {
            _strategies = new Dictionary<EnemyAttackType, IEnemyAttackStrategy>
            {
                [EnemyAttackType.Melee] = new EnemyMeleeAttack(),
                [EnemyAttackType.Ranged] = new EnemyRangedAttack(),
            };
        }

        public IEnemyAttackStrategy Get(EnemyAttackType type) => _strategies[type];
    }
}
