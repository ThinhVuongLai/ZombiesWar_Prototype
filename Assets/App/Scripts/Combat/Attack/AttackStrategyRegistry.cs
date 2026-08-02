using System.Collections.Generic;
using App.Enemy.Weapon;
using ZombiesWar.Bullet;
using ZombiesWar.Weapon;

namespace App.Combat.Attack
{
    public class AttackStrategyRegistry
    {
        readonly Dictionary<WeaponType, IAttackStrategy> _strategies = new();

        public static AttackStrategyRegistry CreateForEnemy(
            EnemyWeaponConfig weaponConfig,
            BulletConfigRegistry bulletRegistry)
        {
            var registry = new AttackStrategyRegistry();
            registry.Register(WeaponType.Melee, new MeleeAttackStrategy());
            registry.Register(WeaponType.Range, new RangedAttackStrategy(null, 0f));
            registry.Register(WeaponType.Throwing, new MeleeAttackStrategy());

            if (weaponConfig != null)
                RegisterFromEnemyConfig(registry, weaponConfig, bulletRegistry);

            return registry;
        }

        public static void RegisterFromEnemyConfig(AttackStrategyRegistry registry, EnemyWeaponConfig config,
            BulletConfigRegistry bulletRegistry)
        {
            switch (config.WeaponType)
            {
                case WeaponType.Range when config is EnemyRangedWeaponConfig rangeConfig:
                    var bulletConfig = bulletRegistry?.GetConfig(rangeConfig.BulletId);
                    registry.Replace(WeaponType.Range, new RangedAttackStrategy(bulletConfig, config.Damage));
                    break;
                case WeaponType.Throwing when config is EnemyThrowWeaponConfig throwConfig:
                    registry.Replace(WeaponType.Throwing, new ThrowAttackStrategy(throwConfig));
                    break;
                default:
                    registry.Replace(WeaponType.Melee, new MeleeAttackStrategy());
                    break;
            }
        }

        public static AttackStrategyRegistry CreateForPlayer(
            int weaponId,
            WeaponConfigRegistry weaponConfigRegistry,
            BulletConfigRegistry bulletRegistry)
        {
            var registry = new AttackStrategyRegistry();

            var config = weaponConfigRegistry?.GetConfig(weaponId);
            if (config != null)
                RegisterFromConfig(registry, config, bulletRegistry);
            else
                registry.Register(WeaponType.Melee, new MeleeAttackStrategy());

            return registry;
        }

        public static void RegisterFromConfig(AttackStrategyRegistry registry, WeaponBase config,
            BulletConfigRegistry bulletRegistry)
        {
            switch (config.WeaponType)
            {
                case WeaponType.Range when config is RangeWeaponConfig rangeConfig:
                    var bulletConfig = bulletRegistry?.GetConfig(rangeConfig.BulletId);
                    registry.Replace(WeaponType.Range, new RangedAttackStrategy(bulletConfig, config.Damage));
                    break;
                case WeaponType.Throwing when config is ThrowWeaponConfig throwConfig:
                    registry.Replace(WeaponType.Throwing, new ThrowAttackStrategy(throwConfig));
                    break;
                default:
                    registry.Replace(WeaponType.Melee, new MeleeAttackStrategy());
                    break;
            }
        }

        public void Register(WeaponType type, IAttackStrategy strategy)
        {
            _strategies[type] = strategy;
        }

        public void Replace(WeaponType type, IAttackStrategy strategy)
        {
            _strategies[type] = strategy;
        }

        public IAttackStrategy Get(WeaponType type)
        {
            _strategies.TryGetValue(type, out var strategy);
            return strategy;
        }
    }
}
