using System;
using System.Collections;
using System.Collections.Generic;
using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy;
using App.Enemy.Wave;
using App.Player;
using App.UI;
using MagicTile.Pool;
using R3;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace App.Level
{
    public enum LevelState { None, Loading, Loaded, Playing, Ended, Cleared }

    public class LevelController : MonoBehaviour
    {
        [SerializeField] LevelState _state;
        [SerializeField] CinemachineVirtualCamera _cinemachineVirtualCamera;
        LevelInfor _currentLevelInfor;
        int _currentLevelId;

        GameObject _playerInstance;
        Scene _loadedLevelScene;
        List<EnemyView> _spawnedEnemies = new();

        int _currentWaveIndex;
        int _totalEnemiesAlive;
        bool _allWavesSpawned;
        CompositeDisposable _levelDisposables;
        CompositeDisposable _waveDisposables;

        LevelConfig _levelConfig;
        PlayerConfig _playerConfig;
        EnemySpawner _enemySpawner;
        IEventBus _eventBus;
        PoolService _poolService;
        PlayerTargetECSUpdater _targetUpdater;

        void Awake()
        {
            ServiceLocator.Register(this);
        }

        void Start()
        {
            _levelConfig = ServiceLocator.Resolve<ConfigManager>().LevelConfig;
            _playerConfig = ServiceLocator.Resolve<ConfigManager>().PlayerConfig;
            _eventBus = ServiceLocator.Resolve<IEventBus>();
            _poolService = ServiceLocator.Resolve<PoolService>();
            _enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        void OnDestroy()
        {
            _levelDisposables?.Dispose();
            _waveDisposables?.Dispose();
        }

        public void LoadLevel(int levelId)
        {
            if (_state != LevelState.None && _state != LevelState.Cleared) return;

            _currentLevelId = levelId;
            _currentLevelInfor = _levelConfig.Levels.Find(l => l.LevelId == levelId);
            if (_currentLevelInfor == null)
            {
                Debug.LogError($"[LevelController] Không tìm thấy LevelInfor với LevelId={levelId}");
                return;
            }

            _state = LevelState.Loading;
            StartCoroutine(LoadLevelCoroutine());
        }

        IEnumerator LoadLevelCoroutine()
        {
            if (!string.IsNullOrEmpty(_currentLevelInfor.SceneName))
            {
                var asyncOp = SceneManager.LoadSceneAsync(_currentLevelInfor.SceneName, LoadSceneMode.Additive);
                if (asyncOp != null)
                {
                    while (!asyncOp.isDone)
                        yield return null;
                    _loadedLevelScene = SceneManager.GetSceneByName(_currentLevelInfor.SceneName);
                }
            }

            var playerInforList = _playerConfig.PlayerInfor;
            var playerHealth = playerInforList != null && playerInforList.Length > 0
                ? playerInforList[0].Health
                : 100f;

            EnemyECSWorldBootstrap.Initialize(playerHealth);
            if (playerInforList != null && playerInforList.Length > 0 && playerInforList[0].PlayerPrefab != null)
            {
                _playerInstance = Instantiate(playerInforList[0].PlayerPrefab, Vector3.zero, Quaternion.identity);
                _targetUpdater = gameObject.AddComponent<PlayerTargetECSUpdater>();
                var targetProvider = _playerInstance.GetComponent<IPlayerTargetProvider>();
                if (targetProvider != null)
                {
                    ServiceLocator.Register<IPlayerTargetProvider>(targetProvider);
                    _targetUpdater.Initialize(targetProvider);
                }

                if (_cinemachineVirtualCamera != null)
                    _cinemachineVirtualCamera.Follow = _playerInstance.transform;
            }

            _state = LevelState.Loaded;
            _eventBus.Publish(new LevelLoadedMessage(_currentLevelId));
        }

        public void StartLevel()
        {
            if (_state != LevelState.Loaded) return;

            _state = LevelState.Playing;
            _levelDisposables?.Dispose();
            _levelDisposables = new CompositeDisposable();

            _currentWaveIndex = 0;
            _totalEnemiesAlive = 0;
            _allWavesSpawned = false;
            _spawnedEnemies.Clear();

            _eventBus.On<EnemyDefeatedMessage>()
                .Subscribe(_ => OnEnemyDefeated())
                .AddTo(_levelDisposables);

            _eventBus.On<PlayerStateUpdatedMessage>()
                .Where(msg => msg.StateType == PlayerStateType.Die)
                .Subscribe(_ => EndLevel(isPlayerWin: false))
                .AddTo(_levelDisposables);

            _eventBus.Publish(new LevelStartedMessage(_currentLevelId));

            ProcessWave(0);
        }

        void ProcessWave(int waveIndex)
        {
            var waveEntries = _currentLevelInfor.WaveEntries;
            if (waveEntries == null || waveIndex >= waveEntries.Count)
            {
                _allWavesSpawned = true;
                return;
            }

            var entry = waveEntries[waveIndex];
            if (entry.EnemyCount <= 0)
            {
                ProcessWave(waveIndex + 1);
                return;
            }

            _currentWaveIndex = waveIndex;
            _totalEnemiesAlive += entry.EnemyCount;

            for (int i = 0; i < entry.EnemyCount; i++)
            {
                var spawnPosition = GetRandomSpawnPosition(entry.SpawnRadius);
                var enemyView = _enemySpawner.SpawnEnemy(spawnPosition, entry.EnemyId);
                if (enemyView != null)
                    _spawnedEnemies.Add(enemyView);
            }

            _waveDisposables?.Dispose();
            _waveDisposables = new CompositeDisposable();

            Observable.Timer(TimeSpan.FromSeconds(entry.IntervalBeforeNext))
                .Subscribe(_ =>
                {
                    if (_state == LevelState.Playing)
                        ProcessWave(_currentWaveIndex + 1);
                })
                .AddTo(_waveDisposables);
        }

        void OnEnemyDefeated()
        {
            _totalEnemiesAlive = Mathf.Max(0, _totalEnemiesAlive - 1);
            if (_totalEnemiesAlive <= 0 && _allWavesSpawned)
                EndLevel(isPlayerWin: true);
        }

        public void EndLevel(bool isPlayerWin)
        {
            if (_state != LevelState.Playing && _state != LevelState.Loaded) return;

            _state = LevelState.Ended;
            _waveDisposables?.Dispose();
            _levelDisposables?.Dispose();
            _eventBus.Publish(new LevelCompletedMessage(_currentLevelId, isPlayerWin));

            var canvasManager = ServiceLocator.Resolve<CanvasManager>();
            if (isPlayerWin)
                canvasManager.Spawn(UIName.WinPopup);
            else
                canvasManager.Spawn(UIName.LosePopup);
        }

        public void ClearLevel()
        {
            var canvasManager = ServiceLocator.Resolve<CanvasManager>();

            foreach (var enemyView in _spawnedEnemies)
            {
                if (enemyView == null) continue;
                if (enemyView.CurrentPresenter is EnemyPresenter presenter)
                    presenter.DestroyECSCombatState();
                enemyView.CurrentPresenter?.Dispose();
                _poolService.Release(enemyView.gameObject);
            }
            _spawnedEnemies.Clear();

            if (_playerInstance != null)
            {
                if (_cinemachineVirtualCamera != null)
                    _cinemachineVirtualCamera.Follow = null;
                ServiceLocator.Unregister<IPlayerTargetProvider>();
                Destroy(_playerInstance);
                _playerInstance = null;
            }

            if (_targetUpdater != null)
            {
                Destroy(_targetUpdater);
                _targetUpdater = null;
            }

            EnemyECSWorldBootstrap.Shutdown();

            if (_loadedLevelScene.isLoaded)
                SceneManager.UnloadSceneAsync(_loadedLevelScene);

            _waveDisposables?.Dispose();
            _levelDisposables?.Dispose();

            _state = LevelState.Cleared;
            _eventBus.Publish(new LevelClearedMessage(_currentLevelId));
        }

        public void ResetLevel()
        {
            ClearLevel();
            LoadLevel(_currentLevelId);

            ServiceLocator.Resolve<IEventBus>().Publish(new ReplayLevelMessage());
        }

        Vector3 GetRandomSpawnPosition(float spawnRadius)
        {
            var center = _playerInstance != null ? _playerInstance.transform.position : Vector3.zero;
            var minimumDistance = spawnRadius * 0.5f;
            var maximumDistance = spawnRadius;

            for (int attempt = 0; attempt < 20; attempt++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var distance = Random.Range(minimumDistance, maximumDistance);
                var candidate = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                if (NavMesh.SamplePosition(candidate, out var hit, 5f, NavMesh.AllAreas))
                    return hit.position;
            }

            var fallbackAngle = Random.Range(0f, Mathf.PI * 2f);
            return center + new Vector3(Mathf.Cos(fallbackAngle) * maximumDistance, 0f, Mathf.Sin(fallbackAngle) * maximumDistance);
        }
    }
}
