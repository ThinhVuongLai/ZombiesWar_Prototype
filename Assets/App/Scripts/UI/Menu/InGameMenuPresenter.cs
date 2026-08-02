using App.Booster;
using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy.Wave;
using App.Level;
using R3;
using Unity.VisualScripting;
using UnityEngine;

namespace App.UI
{
    public class InGameMenuPresenter : ICanvasPresenter
    {
        private readonly InGameMenuView _view;
        private int _totalRemaining;
        private CompositeDisposable _disposables;
        private CanvasManager _canvasManager;
        private LevelController _levelController;
        private IEventBus _eventBus;
        private BoosterManager _boosterManager;
        private int _levelEnemyTotal = 0;

        private float _rocketCooldown = 0;
        private float _lastUseTime;
        private bool _inUseRocket = false;

        public InGameMenuPresenter(InGameMenuView view)
        {
            _view = view;

            _rocketCooldown = ServiceLocator.Resolve<ConfigManager>().BoosterConfig.Cooldown;
        }

        public void Init(params object[] parameters)
        {
            _disposables = new CompositeDisposable();
            _canvasManager = ServiceLocator.Resolve<CanvasManager>();
            _levelController = ServiceLocator.Resolve<LevelController>();
            _eventBus = ServiceLocator.Resolve<IEventBus>();
            _boosterManager = ServiceLocator.Resolve<BoosterManager>();

            _totalRemaining = parameters.Length > 0 && parameters[0] is int total ? total : 0;

            _levelEnemyTotal = _totalRemaining;
            UpdateRemainingText();

            _view.ClickBackToMainAction = OnClickBackToMain;
            _view.ClickUseRocketBoosterAction = OnClickUseRocketBooster;
            _view.ClickChangeWeaponAction = OnClickChangeWeapon;

            _view.SetShowRocketFill(false);
            _inUseRocket = false;
            _lastUseTime = 0;

            _eventBus.On<EnemyDefeatedMessage>()
                .Subscribe(_ =>
                {
                    _totalRemaining = System.Math.Max(0, _totalRemaining - 1);
                    UpdateRemainingText();
                })
                .AddTo(_disposables);

            _eventBus.On<ReplayLevelMessage>()
            .Subscribe(_ =>
            {
                _totalRemaining = _levelEnemyTotal;

                _view.SetShowRocketFill(false);
                _inUseRocket = false;
                _lastUseTime = 0;

                UpdateRemainingText();
            })
            .AddTo(_disposables);

            Observable.EveryUpdate()
                .Subscribe(_ => OnUpdate())
                .AddTo(_disposables);
        }

        private void OnUpdate()
        {
            if (!_inUseRocket)
                return;

            _lastUseTime += Time.deltaTime;
            float percent = _lastUseTime / _rocketCooldown;
            _view.SetFillRocket(percent);

            if (percent >= 1)
            {
                _inUseRocket = false;
                _lastUseTime = 0;
            }
        }

        public void Hide()
        {
            _view.ClickBackToMainAction = null;
            _view.ClickUseRocketBoosterAction = null;
            _view.ClickChangeWeaponAction = null;
            _disposables?.Dispose();
        }

        private void UpdateRemainingText()
            => _view.SetRemainingEnemyText($"Enemies: {_totalRemaining}");

        private void OnClickBackToMain()
        {
            _canvasManager.Hide(UIName.InGameMenu);
            _levelController.ClearLevel();
            _canvasManager.Spawn(UIName.MainMenu);
        }

        private void OnClickUseRocketBooster()
        {
            _boosterManager.UseRocket();

            _view.SetShowRocketFill(true);
            _inUseRocket = true;
        }

        private void OnClickChangeWeapon()
        {
        }
    }
}
