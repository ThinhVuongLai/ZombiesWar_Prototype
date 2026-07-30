using System.Collections.Generic;
using App.Enemy.Weapon;
using ZombiesWar.Bullet;

namespace App.Enemy.Attack
{
    public class AttackStrategyRegistry
    {
        readonly Dictionary<WeaponType, IEnemyAttackStrategy> _strategies;

        public AttackStrategyRegistry(EnemyWeaponConfigRegistry enemyWeaponRegistry, BulletConfigRegistry bulletRegistry)
        {
            _strategies = new Dictionary<WeaponType, IEnemyAttackStrategy>
            {
                [WeaponType.Melee] = new EnemyMeleeAttack(),
            };

            if (enemyWeaponRegistry != null)
            {
                var rangedConfig = enemyWeaponRegistry.GetConfig(WeaponType.Range);
                if (rangedConfig is EnemyRangedWeaponConfig rangedWeapon && bulletRegistry != null)
                {
                    var bulletConfig = bulletRegistry.GetConfig(rangedWeapon.BulletId);
                    _strategies[WeaponType.Range] = new EnemyRangedAttack(bulletConfig, rangedWeapon.Damage);
                }

                var throwConfig = enemyWeaponRegistry.GetConfig(WeaponType.Throwing);
                if (throwConfig is EnemyThrowWeaponConfig throwWeapon)
                {
                    _strategies[WeaponType.Throwing] = new EnemyThrowAttack(throwWeapon);
                }
            }

            if (!_strategies.ContainsKey(WeaponType.Range))
                _strategies[WeaponType.Range] = new EnemyRangedAttack(null, 0f);
            if (!_strategies.ContainsKey(WeaponType.Throwing))
                _strategies[WeaponType.Throwing] = new EnemyMeleeAttack();
        }

        public IEnemyAttackStrategy Get(WeaponType type) => _strategies[type];
    }
}
