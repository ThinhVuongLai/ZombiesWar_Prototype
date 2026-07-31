using App.Core.Services;
using App.Enemy;
using App.Enemy.Weapon;
using App.Enemy.Wave;
using App.HealthBar;
using App.Player;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Weapon;

namespace App.Core
{
    public class ConfigManager : MonoBehaviour
    {
        [SerializeField] PlayerConfig _playerConfig;
        [SerializeField] WeaponConfigRegistry _weaponConfigRegistry;
        [SerializeField] BulletConfigRegistry _bulletConfigRegistry;
        [SerializeField] HealthBarConfig _healthBarConfig;
        [SerializeField] HealthBarConfig _enemyHealthBarConfig;
        [SerializeField] EnemyWeaponConfigRegistry _enemyWeaponConfigRegistry;
        [SerializeField] WaveSpawnerConfig _waveSpawnerConfig;
        [SerializeField] EnemyConfig _enemyConfig;

        public PlayerConfig PlayerConfig => _playerConfig;
        public WeaponConfigRegistry WeaponConfigRegistry => _weaponConfigRegistry;
        public BulletConfigRegistry BulletConfigRegistry => _bulletConfigRegistry;
        public HealthBarConfig HealthBarConfig => _healthBarConfig;
        public HealthBarConfig EnemyHealthBarConfig => _enemyHealthBarConfig;
        public EnemyWeaponConfigRegistry EnemyWeaponConfigRegistry => _enemyWeaponConfigRegistry;
        public WaveSpawnerConfig WaveSpawnerConfig => _waveSpawnerConfig;
        public EnemyConfig EnemyConfig => _enemyConfig;

        void Awake()
        {
            ServiceLocator.Register(this);
        }
    }
}
