using System;
using App.Core.EventBus;

namespace App.Enemy.Wave
{
    public class WaveSpawnerManager : IDisposable
    {
        readonly IEventBus _eventBus;

        WaveSpawnerPresenter _presenter;
        bool _initialized;

        public WaveSpawnerModel Model => _presenter?.Model;

        public WaveSpawnerManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize(IWaveSpawnerView view, WaveSpawnerConfig config, EnemySpawner enemySpawner)
        {
            if (_initialized) return;
            _initialized = true;

            _presenter = new WaveSpawnerPresenter(view, config, enemySpawner, _eventBus);
            _presenter.Initialize();
        }

        public void StartWaves()
        {
            _presenter?.StartWaves();
        }

        public void Dispose()
        {
            _presenter?.Dispose();
        }
    }
}
