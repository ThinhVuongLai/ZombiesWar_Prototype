using App.Core;
using App.Core.Services;
using App.Player.Combat;
using App.Player.ECS;
using MagicTile.Pool;
using R3;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace App.Booster
{
    public class BoosterManager : MonoBehaviour
    {
        BoosterConfig _config;
        PoolService _poolService;
        EntityManager _entityManager;
        Entity _weaponTargetEntity;
        bool _weaponEntityResolved;
        CompositeDisposable _disposables;

        void Awake()
        {
            ServiceLocator.Register(this);
        }

        void Start()
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            _config = configManager.BoosterConfig;
            _poolService = ServiceLocator.Resolve<PoolService>();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _disposables = new CompositeDisposable();

            Observable.EveryUpdate()
                .Subscribe(_ => OnUpdate())
                .AddTo(_disposables);
        }

        void OnUpdate()
        {
            if (_config == null) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                UseRocket();
            }
        }

        public void UseRocket()
        {
            if (_config == null) return;
            
            SpawnRocket(GetTargetPosition());
        }

        Vector3 GetTargetPosition()
        {
            ResolveECSReference();

            if (_weaponEntityResolved && _weaponTargetEntity != Entity.Null
                && _entityManager.Exists(_weaponTargetEntity))
            {
                var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
                if (data.HasTarget)
                {
                    return (Vector3)data.TargetPosition;
                }
            }

            return GetRandomPositionNearPlayer();
        }

        Vector3 GetRandomPositionNearPlayer()
        {
            var playerEntity = PlayerTargetECSUpdater.PlayerEntity;
            if (playerEntity != Entity.Null && _entityManager.Exists(playerEntity))
            {
                var playerPosition = (Vector3)_entityManager.GetComponentData<LocalTransform>(playerEntity).Position;
                var randomAngle = UnityEngine.Random.Range(0f, 360f);
                var direction = new Vector3(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0f,
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                return playerPosition + direction * 3f;
            }

            return Vector3.zero;
        }

        void ResolveECSReference()
        {
            if (!_weaponEntityResolved)
            {
                var query = _entityManager.CreateEntityQuery(typeof(PlayerWeaponTargetData));
                if (query.CalculateEntityCount() > 0)
                {
                    _weaponTargetEntity = query.GetSingletonEntity();
                    _weaponEntityResolved = true;
                }
            }
        }

        void SpawnRocket(Vector3 targetPosition)
        {
            if (_config.RocketPrefab == null || _poolService == null)
                return;

            var rocket = _poolService.Get(_config.RocketPrefab);
            var rocketComponent = rocket.GetComponent<BoosterRocket>();
            if (rocketComponent != null)
            {
                rocketComponent.Initialize(
                    targetPosition,
                    _config.Damage,
                    _config.ExplosionRadius,
                    _config.SpawnHeight,
                    _config.FallSpeed,
                    _config.ExplosionEffectPrefab,
                    _config.EffectDuration);
            }
        }

        void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
