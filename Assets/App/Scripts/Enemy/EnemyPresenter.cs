using System;
using System.Collections.Generic;
using App.Combat.Attack;
using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy.States;
using App.Enemy.Weapon;
using App.Enemy.Wave;
using App.HealthBar;
using R3;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Diagnostics;

namespace App.Enemy
{
    public class EnemyPresenter : IDisposable
    {
        readonly IEnemyView _view;
        readonly EnemyModel _model;
        readonly IPlayerTargetProvider _playerTarget;
        readonly Dictionary<EnemyStateType, IEnemyState> _states;
        readonly CompositeDisposable _disposables = new();
        readonly EntityManager _entityManager;
        readonly AttackStrategyRegistry _attackRegistry;
        readonly WeaponType _attackType;
        readonly string _idleAnimation;
        readonly string _moveAnimation;
        readonly string _attackAnimation;
        readonly string _deadAnimation;

        IEnemyState _currentState;
        Entity _entity;
        HealthBarPresenter _healthBarPresenter;

        public IEnemyView View => _view;
        public IPlayerTargetProvider PlayerTarget => _playerTarget;
        public float AttackDamage { get; }
        public float AttackRange { get; }

        float _lastAttackTime;

        // Cached from ECS (read after SimulationSystemGroup completes, in LateUpdate)
        public EnemyDetectionState CachedDetectionState { get; private set; }

        public EnemyPresenter(IEnemyView view, in EnemyViewConfig config,
            AttackStrategyRegistry registry, EnemyWeaponConfigRegistry enemyWeaponRegistry,
            IPlayerTargetProvider playerTarget)
        {
            _view = view;
            _playerTarget = playerTarget;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _attackRegistry = registry;
            _attackType = config.AttackType;
            _idleAnimation = config.IdleAnimationName;
            _moveAnimation = config.MoveAnimationName;
            _attackAnimation = config.AttackAnimationName;
            _deadAnimation = config.DeadAnimationName;

            var weaponConfig = enemyWeaponRegistry?.GetConfig(config.AttackType);

            _model = new EnemyModel();
            AttackDamage = weaponConfig?.Damage ?? 10f;
            AttackRange = weaponConfig?.AttackRange ?? 2f;

            _model.MoveSpeed.Value = config.MoveSpeed;
            _model.Health.Value = config.Health;
            _model.MaxHealth.Value = config.Health;
            _model.AttackDamage.Value = AttackDamage;
            _model.AttackCooldown.Value = weaponConfig?.AttackCooldown ?? 1.5f;
            _model.DetectionRange.Value = config.DetectionRange;

            _states = new Dictionary<EnemyStateType, IEnemyState>
            {
                [EnemyStateType.Idle] = new EnemyStateIdle(),
                [EnemyStateType.Move] = new EnemyStateMove(),
                [EnemyStateType.Attack] = new EnemyStateAttack(),
                [EnemyStateType.Die] = new EnemyStateDie(),
            };

            TransitionTo(EnemyStateType.Idle);

            _view.OnDestroyed += Dispose;
            _view.TakeExternalDamage = TakeDamage;

            var healthBarView = _view.CreateHealthBar();
            if (healthBarView != null)
            {
                _healthBarPresenter = new HealthBarPresenter(healthBarView, _model.Health, config.Health);
            }

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Subscribe(_ => OnLateUpdate())
                .AddTo(_disposables);
        }

        public void SetEntity(Entity entity)
        {
            _entity = entity;

            var em = _entityManager;

            if (em.HasComponent<EnemyStats>(entity))
            {
                em.SetComponentData(entity, new EnemyStats
                {
                    MoveSpeed = _model.MoveSpeed.Value,
                    AttackDamage = _model.AttackDamage.Value,
                    AttackRange = AttackRange,
                    DetectionRange = _model.DetectionRange.Value,
                    AttackCooldown = _model.AttackCooldown.Value,
                });
            }

            if (em.HasComponent<EnemyHealth>(entity))
            {
                em.SetComponentData(entity, new EnemyHealth { Value = _model.Health.Value });
            }

            if (em.HasComponent<EnemyCombatState>(entity))
            {
                em.SetComponentData(entity, new EnemyCombatState
                {
                    DetectionState = EnemyDetectionState.None,
                });
            }
        }

        void OnLateUpdate()
        {
            if (_entity == Entity.Null || !_entityManager.Exists(_entity))
            {
                _currentState?.Update(this);
                return;
            }

            if (_entityManager.HasComponent<LocalTransform>(_entity))
            {
                var lt = _entityManager.GetComponentData<LocalTransform>(_entity);
                lt.Position = (float3)_view.Transform.position;
                _entityManager.SetComponentData(_entity, lt);
            }

            if (_entityManager.HasComponent<EnemyCombatState>(_entity))
            {
                var combat = _entityManager.GetComponentData<EnemyCombatState>(_entity);
                CachedDetectionState = combat.DetectionState;
            }

            if (_entityManager.HasComponent<EnemyHealth>(_entity))
            {
                var health = _entityManager.GetComponentData<EnemyHealth>(_entity);
                _model.Health.Value = health.Value;
            }

            _currentState?.Update(this);
        }

        public void TransitionTo(EnemyStateType newState)
        {
            _currentState?.Exit(this);
            _currentState = _states[newState];
            _model.CurrentState.Value = newState;

            switch (newState)
            {
                case EnemyStateType.Idle: _view.PlayAnimation(_idleAnimation); break;
                case EnemyStateType.Move: _view.PlayAnimation(_moveAnimation); break;
                case EnemyStateType.Attack: _view.PlayAnimation(_attackAnimation); break;
                case EnemyStateType.Die: _view.PlayAnimation(_deadAnimation); break;
            }

            _currentState.Enter(this);
        }

        public void TakeDamage(float damage)
        {
            if (_entity == Entity.Null || !_entityManager.HasComponent<EnemyHealth>(_entity))
                return;

            var health = _entityManager.GetComponentData<EnemyHealth>(_entity);
            health.Value -= damage;
            _entityManager.SetComponentData(_entity, health);

            UnityEngine.Debug.Log($"Enemy Get Damage, {health}");

            if (health.Value <= 0f)
            {
                ServiceLocator.Resolve<IEventBus>().Publish(new EnemyDefeatedMessage());
                TransitionTo(EnemyStateType.Die);

                UnityEngine.Debug.Log($"Change to Die");
            }
        }

        public bool TryAttack(float currentTime)
        {
            var cooldown = _model.AttackCooldown.Value;
            if (currentTime - _lastAttackTime < cooldown)
                return false;

            _lastAttackTime = currentTime;
            ExecuteAttack();
            _view.PlayAnimation(_attackAnimation);
            return true;
        }

        public void DestroyECSCombatState()
        {
            if (_entity != Entity.Null && _entityManager.Exists(_entity))
                _entityManager.DestroyEntity(_entity);
        }

        public void ExecuteAttack()
        {
            var strategy = _attackRegistry.Get(_attackType);
            if (strategy == null) return;

            strategy.Execute(
                _view.Transform.position, _view.Transform,
                PlayerTargetECSUpdater.PlayerEntity,
                _playerTarget.PlayerTransform.position,
                AttackDamage,
                new PlayerHealthAccessor(),
                faceTarget: true,
                fallbackDamageDealer: dmg =>
                    ServiceLocator.Resolve<IEventBus>()
                        .Publish(new EnemyDealtDamageMessage(dmg, _attackType)));
        }

        public void Dispose()
        {
            _view.OnDestroyed -= Dispose;
            _view.TakeExternalDamage = null;
            _healthBarPresenter?.Dispose();
            _disposables.Dispose();
        }
    }
}