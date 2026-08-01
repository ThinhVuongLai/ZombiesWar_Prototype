using System;
using System.Collections.Generic;
using App.Combat.Attack;
using App.Core;
using App.Core.EventBus;
using App.HealthBar;
using App.Player.Combat;
using App.Player.ECS;
using App.Player.States;
using R3;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Weapon;

namespace App.Player
{
    public class PlayerPresenter : IDisposable
    {
        readonly IPlayerView _view;
        readonly PlayerConfig _playerConfig;
        readonly PlayerModel _model;
        readonly IPlayerInputProvider _input;
        readonly IEventBus _eventBus;
        readonly Dictionary<PlayerStateType, IPlayerState> _states;
        readonly CompositeDisposable _disposables = new();
        readonly PlayerCombatModel _combatModel;
        readonly EntityManager _entityManager;
        readonly WeaponConfigRegistry _weaponConfigRegistry;
        readonly BulletConfigRegistry _bulletConfigRegistry;
        readonly AttackStrategyRegistry _attackRegistry;

        Entity _weaponTargetEntity;
        bool _weaponEntityResolved;
        bool _hasUpdateWeapon;

        Entity _playerHealthEntity;
        bool _playerHealthResolved;

        BulletConfig _currentBulletConfig;
        WeaponBase _currentWeaponConfig;
        WeaponType _currentWeaponType;
        float _lastAttackTime;

        IPlayerState _currentState;
        HealthBarPresenter _healthBarPresenter;
        bool _hadCombatTarget;

        public const float Gravity = -9.81f;
        public float VerticalVelocity { get; set; }

        public IPlayerView View => _view;
        public IPlayerInputProvider Input => _input;
        public float MoveSpeed => _model.MoveSpeed.Value;
        public bool IsAlive => _model.IsAlive.Value;
        public bool ShouldAttack { get; set; }

        public Vector3 CombatTargetDirection => _combatModel.TargetDirection.Value;
        public bool HasCombatTarget => _combatModel.HasTarget.Value;

        public PlayerPresenter(IPlayerView view, PlayerConfig playerConfig, IPlayerInputProvider input, IEventBus eventBus,
            WeaponConfigRegistry weaponConfigRegistry, BulletConfigRegistry bulletConfigRegistry)
        {
            _view = view;
            _playerConfig = playerConfig;
            _input = input;
            _eventBus = eventBus;
            _weaponConfigRegistry = weaponConfigRegistry;
            _bulletConfigRegistry = bulletConfigRegistry;
            _attackRegistry = new AttackStrategyRegistry();
            _attackRegistry.Register(WeaponType.Melee, new MeleeAttackStrategy());

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

            var healthBarView = _view.CreateHealthBar();
            if (healthBarView != null)
            {
                _healthBarPresenter = new HealthBarPresenter(
                    healthBarView, _model.Health, _model.MaximumHealth.Value);
            }

            Observable.EveryUpdate()
                .Subscribe(_ => OnUpdate())
                .AddTo(_disposables);

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Subscribe(_ => OnLateUpdate())
                .AddTo(_disposables);

            _eventBus.On<EnemyDealtDamageMessage>().Subscribe(message =>
            {
                if (_playerHealthEntity == Entity.Null)
                    return;

                var health = _entityManager.GetComponentData<PlayerHealth>(_playerHealthEntity);
                health.Value = math.max(health.Value - message.Damage, 0f);
                _entityManager.SetComponentData(_playerHealthEntity, health);
            }).AddTo(_disposables);

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

            if(UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                PlayAttackAnimation();
            }
        }

        void OnLateUpdate()
        {
            ResolveECSReferences();
            
            bool hadTarget = _hadCombatTarget;
            _hadCombatTarget = HasCombatTarget;
            
            SyncCombatTarget();
            SyncPlayerHealth();
            
            if (!hadTarget && HasCombatTarget)
            {
                PlayAttackAnimation();
            }
            else if (hadTarget && !HasCombatTarget)
            {
                PlayAttackIdleAnimation();
            }
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

            if (!_hasUpdateWeapon)
            {
                if (_currentWeaponConfig != null)
                {
                    UpdateWeaponECSRadius(_currentWeaponConfig.AttackRange);
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
            _model.MaximumHealth.Value = health.MaxValue;

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
            
            PlayStateAnimation(newState);
        }
        
        void PlayStateAnimation(PlayerStateType state)
        {
            if (_playerConfig == null) return;
            
            var animationName = state switch
            {
                PlayerStateType.Idle => _playerConfig.IdleAnimation,
                PlayerStateType.Move => _playerConfig.MoveAnimation,
                _ => null,
            };
            
            if (!string.IsNullOrEmpty(animationName))
            {
                _view.PlayMoveAnimation(animationName, _playerConfig.MoveAnimationLayerIndex);
            }
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
            }
            else if (weaponConfig is ThrowWeaponConfig)
            {
                _currentBulletConfig = null;
                _combatModel.AttackRadius.Value = weaponConfig.AttackRange;
            }
            else
            {
                _currentBulletConfig = null;
                _combatModel.AttackRadius.Value = weaponConfig.AttackRange;
            }

            UpdateWeaponECSRadius(weaponConfig.AttackRange);

            AttackStrategyRegistry.RegisterFromConfig(_attackRegistry, weaponConfig, _bulletConfigRegistry);
            
            PlayAttackIdleAnimation();
        }

        void UpdateWeaponECSRadius(float radius)
        {
            if (!_hasUpdateWeapon && _weaponTargetEntity != Entity.Null)
            {
                _entityManager.SetComponentData(_weaponTargetEntity, new PlayerWeaponTargetData
                {
                    AttackRadius = radius,
                    CurrentTargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    TargetDirection = float3.zero,
                    HasTarget = false,
                });

                _hasUpdateWeapon = true;
            }
        }

        void PlayAttackAnimation()
        {
            if (_playerConfig == null || _currentWeaponConfig == null) return;
            var animationName = _currentWeaponConfig.AttackAnimation;
            if (!string.IsNullOrEmpty(animationName))
            {
                _view.PlayAttackAnimation(animationName, _playerConfig.AttackAnimationLayerIndex);
            }
        }
        
        void PlayAttackIdleAnimation()
        {
            if (_playerConfig == null || _currentWeaponConfig == null) return;
            var animationName = _currentWeaponConfig.AttackIdleAnimation;
            if (!string.IsNullOrEmpty(animationName))
            {
                _view.PlayAttackAnimation(animationName, _playerConfig.AttackAnimationLayerIndex);
            }
        }

        public void TryAttack()
        {
            if (Time.time - _lastAttackTime < _combatModel.AttackCooldown.Value) return;
            if (!_weaponEntityResolved || _weaponTargetEntity == Entity.Null) return;

            var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
            if (!data.HasTarget) return;

            var targetEntity = data.CurrentTargetEntity;
            if (targetEntity == Entity.Null || !_entityManager.Exists(targetEntity)) return;
            if (!_entityManager.HasComponent<LocalTransform>(targetEntity)) return;

            var targetPosition = (Vector3)_entityManager.GetComponentData<LocalTransform>(targetEntity).Position;

            var strategy = _attackRegistry.Get(_currentWeaponType);
            if (strategy == null) return;

            strategy.Execute(
                _view.Transform.position, _view.Transform,
                targetEntity, targetPosition,
                _currentWeaponConfig.Damage,
                new EnemyHealthAccessor(),
                faceTarget: false);

            _lastAttackTime = Time.time;
            
            PlayAttackAnimation();
        }

        public void Dispose()
        {
            _healthBarPresenter?.Dispose();
            _disposables.Dispose();
        }
    }
}