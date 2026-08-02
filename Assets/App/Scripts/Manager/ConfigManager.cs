using App.Audio;
using App.Booster;
using App.Core.Services;
using App.Enemy;
using App.Enemy.Weapon;
using App.Enemy.Wave;
using App.HealthBar;
using App.Level;
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
        [SerializeField] LevelConfig _levelConfig;
        [SerializeField] GlobalData _globalData;
        [SerializeField] BoosterConfig _boosterConfig;
        [SerializeField] MusicConfig _musicConfig;
        [SerializeField] SfxConfig _sfxConfig;

        public PlayerConfig PlayerConfig => _playerConfig;
        public WeaponConfigRegistry WeaponConfigRegistry => _weaponConfigRegistry;
        public BulletConfigRegistry BulletConfigRegistry => _bulletConfigRegistry;
        public HealthBarConfig HealthBarConfig => _healthBarConfig;
        public HealthBarConfig EnemyHealthBarConfig => _enemyHealthBarConfig;
        public EnemyWeaponConfigRegistry EnemyWeaponConfigRegistry => _enemyWeaponConfigRegistry;
        public WaveSpawnerConfig WaveSpawnerConfig => _waveSpawnerConfig;
        public EnemyConfig EnemyConfig => _enemyConfig;
        public LevelConfig LevelConfig => _levelConfig;
        public GlobalData GlobalData => _globalData;
        public BoosterConfig BoosterConfig => _boosterConfig;
        public MusicConfig MusicConfig => _musicConfig;
        public SfxConfig SfxConfig => _sfxConfig;

        void Awake()
        {
            ServiceLocator.Register(this);
        }
    }
}
