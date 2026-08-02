using App.Booster;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy.Wave;
using App.Level;
using R3;

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

        public InGameMenuPresenter(InGameMenuView view)
        {
            _view = view;
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
                UpdateRemainingText();
            })
            .AddTo(_disposables);
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
        }

        private void OnClickChangeWeapon()
        {
        }
    }
}
