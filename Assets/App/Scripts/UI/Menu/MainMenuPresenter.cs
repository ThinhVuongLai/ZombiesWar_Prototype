using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Level;
using R3;
using System;
using System.Collections.Generic;

namespace App.UI
{
    public class MainMenuPresenter : ICanvasPresenter
    {
        private readonly MainMenuView _view;
        private readonly CanvasManager _canvasManager;
        private readonly LevelController _levelController;
        private readonly IEventBus _eventBus;
        private readonly LevelConfig _levelConfig;
        private CompositeDisposable _disposables = new();

        public MainMenuPresenter(MainMenuView view)
        {
            _view = view;
            _canvasManager = ServiceLocator.Resolve<CanvasManager>();
            _levelController = ServiceLocator.Resolve<LevelController>();
            _eventBus = ServiceLocator.Resolve<IEventBus>();
            _levelConfig = ServiceLocator.Resolve<ConfigManager>().LevelConfig;
        }

        public void Init(params object[] parameters)
        {
            _disposables = new CompositeDisposable();

            var levelIds = new List<int>();
            foreach (var level in _levelConfig.Levels)
                levelIds.Add(level.LevelId);
            _view.SetLevelItems(levelIds);

            _view.ClickLevelItemAction = OnClickLevelItem;
        }

        public void Hide()
        {
            _view.ClickLevelItemAction = null;
            _disposables?.Dispose();
        }

        private void OnClickLevelItem(int levelId)
        {
            _canvasManager.Spawn(UIName.LoadingPopup);

            _canvasManager.Hide(UIName.MainMenu);
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _levelController.LoadLevel(levelId);

            _eventBus.On<LevelLoadedMessage>()
                .Where(msg => msg.LevelId == levelId)
                .Take(1)
                .Subscribe(_ =>
                {
                    // _levelController.StartLevel();

                    var levelInfor = _levelConfig.Levels.Find(l => l.LevelId == levelId);
                    int totalEnemies = 0;
                    if (levelInfor?.WaveEntries != null)
                        foreach (var entry in levelInfor.WaveEntries)
                            totalEnemies += entry.EnemyCount;

                    _canvasManager.Spawn(UIName.InGameMenu, totalEnemies);
                })
                .AddTo(_disposables);
        }
    }
}
