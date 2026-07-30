using System;
using System.Collections.Generic;
using App.Core;
using App.Core.EventBus;
using App.Player.Combat;
using App.Player.ECS;
using App.Player.States;
using R3;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Bullet.ECS;
using ZombiesWar.ThrowingWeapon;
using ZombiesWar.Weapon;

namespace App.Player
{
    public class PlayerPresenter : IDisposable
    {
        readonly IPlayerView _view;
        readonly PlayerModel _model;
        readonly IPlayerInputProvider _input;
        readonly IEventBus _eventBus;
        readonly Dictionary<PlayerStateType, IPlayerState> _states;
        readonly CompositeDisposable _disposables = new();
        readonly PlayerCombatModel _combatModel;
        readonly EntityManager _entityManager;
        readonly WeaponConfigRegistry _weaponConfigRegistry;
        readonly BulletConfigRegistry _bulletConfigRegistry;
        readonly ThrowActionRegistry _throwActionRegistry;

        Entity _weaponTargetEntity;
        bool _weaponEntityResolved;

        Entity _playerHealthEntity;
        bool _playerHealthResolved;

        BulletConfig _currentBulletConfig;
        WeaponBase _currentWeaponConfig;
        WeaponType _currentWeaponType;
        float _currentMeleeDamage;
        float _currentMeleeRange;
        float _lastAttackTime;

        IPlayerState _currentState;

        public const float Gravity = -9.81f;
        public float VerticalVelocity { get; set; }

        public IPlayerView View => _view;
        public IPlayerInputProvider Input => _input;
        public float MoveSpeed => _model.MoveSpeed.Value;
        public bool IsAlive => _model.IsAlive.Value;
        public bool ShouldAttack { get; set; }

        public Vector3 CombatTargetDirection => _combatModel.TargetDirection.Value;
        public bool HasCombatTarget => _combatModel.HasTarget.Value;

        public PlayerPresenter(IPlayerView view, IPlayerInputProvider input, IEventBus eventBus,
            WeaponConfigRegistry weaponConfigRegistry, BulletConfigRegistry bulletConfigRegistry)
        {
            _view = view;
            _input = input;
            _eventBus = eventBus;
            _weaponConfigRegistry = weaponConfigRegistry;
            _bulletConfigRegistry = bulletConfigRegistry;
            _throwActionRegistry = new ThrowActionRegistry();

            _model = new PlayerModel();
            _combatModel = new PlayerCombatModel();

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _states = new Dictionary<PlayerStateType, IPlayerState>
            {
                [PlayerStateType.Idle] = new PlayerStateIdle(),
                [PlayerStateType.Move] = new PlayerStateMove(),
                [PlayerStateType.Die] = new PlayerStateDie(),
            };

            TransitionTo(PlayerStateType.Idle);

            Observable.EveryUpdate()
                .Subscribe(_ => OnUpdate())
                .AddTo(_disposables);

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Subscribe(_ => OnLateUpdate())
                .AddTo(_disposables);

            SetWeapon(0);
        }

        void OnUpdate()
        {
            _currentState?.Update(this);

            if (HasCombatTarget)
            {
                PlayerStateType state = _model.CurrentState.Value;
                if (state == PlayerStateType.Idle || state == PlayerStateType.Move)
                {
                    TryAttack();
                }
            }

            _eventBus.Publish(new PlayerStateUpdatedMessage(_model.CurrentState.Value));
        }

        void OnLateUpdate()
        {
            ResolveECSReferences();
            SyncCombatTarget();
            SyncPlayerHealth();
        }

        void ResolveECSReferences()
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

            if (!_playerHealthResolved && PlayerTargetECSUpdater.PlayerEntity != Entity.Null)
            {
                _playerHealthEntity = PlayerTargetECSUpdater.PlayerEntity;
                _playerHealthResolved = true;
            }
        }

        void SyncCombatTarget()
        {
            if (_weaponTargetEntity == Entity.Null) return;

            var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
            _combatModel.HasTarget.Value = data.HasTarget;
            _combatModel.TargetDirection.Value = data.TargetDirection;
        }

        void SyncPlayerHealth()
        {
            if (_playerHealthEntity == Entity.Null || !_entityManager.Exists(_playerHealthEntity))
                return;

            var health = _entityManager.GetComponentData<PlayerHealth>(_playerHealthEntity);
            _model.Health.Value = health.Value;
            _model.MaxHealth.Value = health.MaxValue;

            if (health.Value <= 0f && _model.IsAlive.Value)
            {
                Die();
            }
        }

        public void TransitionTo(PlayerStateType newState)
        {
            _currentState?.Exit(this);
            _currentState = _states[newState];
            _model.CurrentState.Value = newState;
            _currentState.Enter(this);
        }

        public void Die()
        {
            _model.IsAlive.Value = false;
            TransitionTo(PlayerStateType.Die);
        }

        public void SetWeapon(int weaponId)
        {
            var weaponConfig = _weaponConfigRegistry?.GetConfig(weaponId);
            if (weaponConfig == null)
            {
                _currentBulletConfig = null;
                _currentWeaponConfig = null;
                _currentWeaponType = WeaponType.Melee;
                return;
            }

            _currentWeaponConfig = weaponConfig;
            _currentWeaponType = weaponConfig.WeaponType;
            _combatModel.AttackCooldown.Value = weaponConfig.AttackCooldown;

            if (weaponConfig is RangeWeaponConfig rangeConfig)
            {
                _currentBulletConfig = _bulletConfigRegistry?.GetConfig(rangeConfig.BulletId);
                _combatModel.AttackRadius.Value = weaponConfig.AttackRange;
                UpdateWeaponECSRadius(weaponConfig.AttackRange);
            }
            else if (weaponConfig is ThrowWeaponConfig)
            {
                _currentBulletConfig = null;
                _combatModel.AttackRadius.Value = weaponConfig.AttackRange;
                UpdateWeaponECSRadius(weaponConfig.AttackRange);
            }
            else
            {
                _currentBulletConfig = null;
                _currentMeleeDamage = weaponConfig.Damage;
                _currentMeleeRange = weaponConfig.AttackRange;
                _combatModel.AttackRadius.Value = weaponConfig.AttackRange;
                UpdateWeaponECSRadius(weaponConfig.AttackRange);
            }
        }

        void UpdateWeaponECSRadius(float radius)
        {
            if (_weaponEntityResolved && _weaponTargetEntity != Entity.Null)
            {
                _entityManager.SetComponentData(_weaponTargetEntity, new PlayerWeaponTargetData
                {
                    AttackRadius = radius,
                    CurrentTargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    TargetDirection = float3.zero,
                    HasTarget = false,
                });
            }
        }

        public void TryAttack()
        {
            if (Time.time - _lastAttackTime < _combatModel.AttackCooldown.Value) return;
            if (!_weaponEntityResolved || _weaponTargetEntity == Entity.Null) return;

            var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
            if (!data.HasTarget) return;

            if (_currentWeaponType == WeaponType.Melee)
            {
                TryMeleeAttack(data);
            }
            else if (_currentWeaponType == WeaponType.Throwing && _currentWeaponConfig is ThrowWeaponConfig)
            {
                TryThrowingAttack(data);
            }
            else if (_currentBulletConfig != null)
            {
                TryRangedAttack(data);
            }
        }

        void TryMeleeAttack(PlayerWeaponTargetData data)
        {
            var targetEntity = data.CurrentTargetEntity;
            if (targetEntity == Entity.Null || !_entityManager.Exists(targetEntity))
                return;

            if (!_entityManager.HasComponent<EnemyHealth>(targetEntity))
                return;

            var targetTransform = _entityManager.GetComponentData<LocalTransform>(targetEntity);
            var sqrDist = math.distancesq(targetTransform.Position, (float3)_view.Transform.position);

            if (sqrDist > _currentMeleeRange * _currentMeleeRange)
                return;

            _lastAttackTime = Time.time;

            var health = _entityManager.GetComponentData<EnemyHealth>(targetEntity);
            health.Value = math.max(health.Value - _currentMeleeDamage, 0f);
            _entityManager.SetComponentData(targetEntity, health);
        }

        void TryRangedAttack(PlayerWeaponTargetData data)
        {
            _lastAttackTime = Time.time;

            var firePos = (float3)_view.Transform.position + new float3(0, 1.5f, 0);
            BulletSpawner.SpawnBullet(_currentBulletConfig, _currentWeaponConfig.Damage,
                firePos, data.CurrentTargetEntity);
        }

        void TryThrowingAttack(PlayerWeaponTargetData data)
        {
            _lastAttackTime = Time.time;

            var config = _currentWeaponConfig as ThrowWeaponConfig;
            if (config == null) return;
            var targetEntity = data.CurrentTargetEntity;
            if (targetEntity == Entity.Null || !_entityManager.Exists(targetEntity))
                return;
            if (!_entityManager.HasComponent<LocalTransform>(targetEntity))
                return;

            var targetPos = _entityManager.GetComponentData<LocalTransform>(targetEntity).Position;
            var throwPos = (Vector3)(float3)_view.Transform.position + new Vector3(0, 1.5f, 0);

            var horizontalDist = Vector3.Distance(
                new Vector3(throwPos.x, 0, throwPos.z),
                new Vector3(targetPos.x, 0, targetPos.z));
            var heightDiff = targetPos.y - throwPos.y;

            var angleRad = config.ThrowAngle * Mathf.Deg2Rad;
            var angleCos = Mathf.Cos(angleRad);
            var angleSin = Mathf.Sin(angleRad);

            var denominator = 2f * (horizontalDist * angleSin * angleCos -
                heightDiff * angleCos * angleCos);
            if (Mathf.Abs(denominator) < 0.001f) return;

            var gMagnitude = Mathf.Abs(Physics.gravity.y) * config.GravityScale;
            var speedSq = (gMagnitude * horizontalDist * horizontalDist) / denominator;
            if (speedSq <= 0f) return;
            var speed = Mathf.Sqrt(speedSq);

            speed = Mathf.Clamp(speed, config.MinThrowForce, config.MaxThrowForce);

            var dirToTarget = new Vector3(targetPos.x - throwPos.x, 0, targetPos.z - throwPos.z).normalized;
            var velocity = dirToTarget * (speed * angleCos);
            velocity.y = speed * angleSin;

            var thrownGo = UnityEngine.Object.Instantiate(config.ObjectPrefab, throwPos, Quaternion.identity);
            var thrownObj = thrownGo.GetComponent<ThrownObject>();
            if (thrownObj == null)
            {
                thrownObj = thrownGo.AddComponent<ThrownObject>();
            }

            var throwAction = _throwActionRegistry.GetAction(config.ActionType);
            thrownObj.Initialize(config.ObjectLifespan, config.ActionRadius, config.Damage,
                config.GravityScale, throwAction, velocity);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}