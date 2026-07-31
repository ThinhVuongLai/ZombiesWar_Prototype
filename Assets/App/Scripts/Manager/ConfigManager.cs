using App.Core.Services;
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
        [SerializeField] EnemyWeaponConfigRegistry _enemyWeaponConfigRegistry;
        [SerializeField] WaveSpawnerConfig _waveSpawnerConfig;

        public PlayerConfig PlayerConfig => _playerConfig;
        public WeaponConfigRegistry WeaponConfigRegistry => _weaponConfigRegistry;
        public BulletConfigRegistry BulletConfigRegistry => _bulletConfigRegistry;
        public HealthBarConfig HealthBarConfig => _healthBarConfig;
        public EnemyWeaponConfigRegistry EnemyWeaponConfigRegistry => _enemyWeaponConfigRegistry;
        public WaveSpawnerConfig WaveSpawnerConfig => _waveSpawnerConfig;

        void Awake()
        {
            ServiceLocator.Register(this);
        }
    }
}
