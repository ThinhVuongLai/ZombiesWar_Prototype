using App.Core.Installers;
using App.Core.Services;
using App.Enemy;
using App.Enemy.Wave;
using App.JoystickInput;
using App.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZombiesWar.Bullet;

namespace App.Core
{
    public class GameManager : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new CoreInstaller().Install(builder);
            new JoystickInputInstaller().Install(builder);
            new PlayerInstaller().Install(builder);
            new EnemyInstaller().Install(builder);
            new WaveInstaller().Install(builder);
            new BulletInstaller().Install(builder);
        }

        void Start()
        {
            EnemyECSWorldBootstrap.Initialize();

            var playerTarget = ServiceLocator.Resolve<IPlayerTargetProvider>();
            var updater = gameObject.AddComponent<PlayerTargetECSUpdater>();
            updater.Initialize(playerTarget);

            StartCoroutine(CRStartWaves());
        }

        private System.Collections.IEnumerator CRStartWaves()
        {
            yield return null;
            ServiceLocator.Resolve<WaveSpawnerManager>().StartWaves();
        }
    }
}
