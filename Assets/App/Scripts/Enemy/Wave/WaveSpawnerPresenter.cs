using System;
using App.Core.EventBus;
using App.Player;
using R3;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace App.Enemy.Wave
{
    public class WaveSpawnerPresenter : IDisposable
    {
        readonly IWaveSpawnerView _view;
        readonly WaveSpawnerConfig _config;
        readonly EnemySpawner _enemySpawner;
        readonly IEventBus _eventBus;
        readonly CompositeDisposable _disposables = new();

        WaveSpawnerModel _model;
        CompositeDisposable _timerDisposables = new();

        public WaveSpawnerModel Model => _model;
        public bool IsInitialized { get; private set; }

        public WaveSpawnerPresenter(IWaveSpawnerView view, WaveSpawnerConfig config,
            EnemySpawner enemySpawner, IEventBus eventBus)
        {
            _view = view;
            _config = config;
            _enemySpawner = enemySpawner;
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;

            _model = new WaveSpawnerModel();

            _eventBus.On<EnemyDefeatedMessage>()
                .Subscribe(OnEnemyDefeated)
                .AddTo(_disposables);

            if (_config == null || _config.Waves == null || _config.WaveCount == 0)
            {
                _model.State.Value = WaveSpawnerState.Completed;
                return;
            }
        }

        public void StartWaves()
        {
            if (!IsInitialized) Initialize();
            if (_model.State.Value == WaveSpawnerState.Completed) return;

            _model.IsActive.Value = true;
            ProcessWave(0);
        }

        void ProcessWave(int index)
        {
            if (index >= _config.WaveCount)
            {
                CompleteAllWaves();
                return;
            }

            var waveConfig = _config.GetWave(index);
            if (waveConfig == null || waveConfig.EnemyCount <= 0)
            {
                ProcessWave(index + 1);
                return;
            }

            _model.CurrentWaveIndex.Value = index;
            _model.State.Value = WaveSpawnerState.Spawning;
            _model.EnemiesAlive.Value = waveConfig.EnemyCount;

            _eventBus.Publish(new WaveStartedMessage(index, waveConfig.EnemyCount));

            for (int i = 0; i < waveConfig.EnemyCount; i++)
            {
                var spawnPosition = GetRandomSpawnPosition(waveConfig.SpawnRadius);
                _enemySpawner.SpawnEnemy(spawnPosition, waveConfig.EnemyId);
                _eventBus.Publish(new EnemySpawnedMessage());
            }

            _model.State.Value = WaveSpawnerState.Active;
            StartWaveTimer(waveConfig.IntervalBeforeNext, index + 1);
        }

        void StartWaveTimer(float interval, int nextWaveIndex)
        {
            _timerDisposables.Clear();
            _timerDisposables = new CompositeDisposable();

            var startTime = Time.time;
            var totalInterval = interval;

            Observable.EveryUpdate()
                .TakeWhile(_ => _model.IsActive.Value)
                .Subscribe(_ =>
                {
                    var elapsed = Time.time - startTime;
                    _model.WaveTimer.Value = Mathf.Max(0f, totalInterval - elapsed);
                })
                .AddTo(_timerDisposables);

            Observable.Timer(TimeSpan.FromSeconds(interval))
                .Take(1)
                .SelectMany(_ => _eventBus.On<PlayerStateUpdatedMessage>().Take(1))
                .Subscribe(_ =>
                {
                    ProcessWave(nextWaveIndex);
                })
                .AddTo(_timerDisposables);
        }

        void CompleteAllWaves()
        {
            _timerDisposables.Clear();
            _model.State.Value = WaveSpawnerState.Completed;
            _model.IsActive.Value = false;
            _model.WaveTimer.Value = 0f;
            _eventBus.Publish(new AllWavesCompletedMessage());
        }

        void OnEnemyDefeated(EnemyDefeatedMessage message)
        {
            if (_model.State.Value != WaveSpawnerState.Active) return;

            _model.EnemiesAlive.Value = Mathf.Max(0, _model.EnemiesAlive.Value - 1);

            if (_model.EnemiesAlive.Value == 0)
            {
                _eventBus.Publish(new WaveCompletedMessage(_model.CurrentWaveIndex.Value));
            }
        }

        Vector3 GetRandomSpawnPosition(float spawnRadius)
        {
            var extents = _view.ScreenExtents;
            var screenRadius = Mathf.Max(extents.x, extents.y);
            const float navMeshSampleRadius = 3f;
            const float screenMargin = 1.05f;
            var minimumDistance = screenRadius * screenMargin + navMeshSampleRadius;
            var maximumDistance = Mathf.Max(minimumDistance + 1f, spawnRadius);

            var center = _view.WorldCenter;
            const int maximumAttempts = 20;

            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var distance = Random.Range(minimumDistance, maximumDistance);

                var candidate = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                if (NavMesh.SamplePosition(candidate, out var hit, navMeshSampleRadius, NavMesh.AllAreas))
                {
                    if (IsOutsideScreenExtents(hit.position, center, extents))
                        return hit.position;
                }
            }

            // Fallback: place at maxDistance far outside screen
            var fallbackAngle = Random.Range(0f, Mathf.PI * 2f);
            return center + new Vector3(Mathf.Cos(fallbackAngle) * maximumDistance, 0f, Mathf.Sin(fallbackAngle) * maximumDistance);
        }

        bool IsOutsideScreenExtents(Vector3 position, Vector3 center, Vector2 extents)
        {
            var dx = Mathf.Abs(position.x - center.x);
            var dz = Mathf.Abs(position.z - center.z);
            return dx > extents.x || dz > extents.y;
        }

        public void Dispose()
        {
            _timerDisposables.Dispose();
            _disposables.Dispose();
        }
    }
}
