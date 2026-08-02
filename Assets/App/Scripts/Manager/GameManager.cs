using App.Core.Installers;
using App.Core.Services;
using App.Enemy;
using App.Enemy.Wave;
using App.JoystickInput;
using App.Level;
using App.Player;
using App.UI;
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

        private void Start()
        {
            ServiceLocator.Resolve<CanvasManager>().Spawn(uiName: UIName.MainMenu);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                LoadLevel();
            }

            if(Input.GetKeyDown(KeyCode.S))
            {
                StartLevel();
            }
        }

        private void LoadLevel()
        {
            var LevelController = ServiceLocator.Resolve<LevelController>();

            if (LevelController == null)
                return;

            LevelController.LoadLevel(0);
        }

        private void StartLevel()
        {
            var LevelController = ServiceLocator.Resolve<LevelController>();

            if (LevelController == null)
                return;

            LevelController.StartLevel();
        }
    }
}
