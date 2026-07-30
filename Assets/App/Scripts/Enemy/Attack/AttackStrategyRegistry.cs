using System.Collections.Generic;
using App.Enemy;
using App.Enemy.Weapon;
using ZombiesWar.Bullet;

namespace App.Enemy.Attack
{
    public class AttackStrategyRegistry
    {
        readonly Dictionary<EnemyAttackType, IEnemyAttackStrategy> _strategies;

        public AttackStrategyRegistry(EnemyWeaponConfigRegistry enemyWeaponRegistry, BulletConfigRegistry bulletRegistry)
        {
            _strategies = new Dictionary<EnemyAttackType, IEnemyAttackStrategy>
            {
                [EnemyAttackType.Melee] = new EnemyMeleeAttack(),
            };

            if (enemyWeaponRegistry != null)
            {
                var rangedConfig = enemyWeaponRegistry.GetConfig(EnemyAttackType.Ranged);
                if (rangedConfig != null && bulletRegistry != null)
                {
                    var bulletConfig = bulletRegistry.GetConfig(rangedConfig.BulletId);
                    _strategies[EnemyAttackType.Ranged] = new EnemyRangedAttack(bulletConfig, rangedConfig.AttackDamage);
                }
            }

            if (!_strategies.ContainsKey(EnemyAttackType.Ranged))
            {
                _strategies[EnemyAttackType.Ranged] = new EnemyRangedAttack(null, 0f);
            }
        }

        public IEnemyAttackStrategy Get(EnemyAttackType type) => _strategies[type];
    }
}
