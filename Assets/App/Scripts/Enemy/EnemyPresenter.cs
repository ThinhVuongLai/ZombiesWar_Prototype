using System;
using System.Collections.Generic;
using App.Combat.Attack;
using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy.States;
using App.Enemy.Weapon;
using App.Enemy.Wave;
using R3;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

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

        IEnemyState _currentState;
        Entity _entity;

        public IEnemyView View => _view;
        public IPlayerTargetProvider PlayerTarget => _playerTarget;
        public float AttackDamage { get; }
        public float AttackRange { get; }

        // Cached from ECS (read after SimulationSystemGroup completes, in LateUpdate)
        public EnemyDetectionState CachedDetectionState { get; private set; }
        public bool CachedNeedsCombatResult { get; private set; }

        public EnemyPresenter(IEnemyView view, in EnemyViewConfig config,
            AttackStrategyRegistry registry, EnemyWeaponConfigRegistry enemyWeaponRegistry,
            IPlayerTargetProvider playerTarget)
        {
            _view = view;
            _playerTarget = playerTarget;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _attackRegistry = registry;
            _attackType = config.AttackType;

            var weaponConfig = enemyWeaponRegistry?.GetConfig(config.AttackType);

            _model = new EnemyModel();
            AttackDamage = weaponConfig?.Damage ?? 10f;
            AttackRange = weaponConfig?.AttackRange ?? 2f;

            _model.MoveSpeed.Value = config.MoveSpeed;
            _model.Health.Value = config.Health;
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
                    LastAttackTime = 0f,
                    DetectionState = EnemyDetectionState.None,
                    NeedsCombatResult = false,
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
                CachedNeedsCombatResult = combat.NeedsCombatResult;
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
            _currentState.Enter(this);
        }

        public void TakeDamage(float damage)
        {
            if (_entity == Entity.Null || !_entityManager.HasComponent<EnemyHealth>(_entity))
                return;

            var health = _entityManager.GetComponentData<EnemyHealth>(_entity);
            health.Value -= damage;
            _entityManager.SetComponentData(_entity, health);

            if (health.Value <= 0f)
            {
                ServiceLocator.Resolve<IEventBus>().Publish(new EnemyDefeatedMessage());
                TransitionTo(EnemyStateType.Die);
            }
        }

        public void ResetNeedsCombatResult()
        {
            if (_entity == Entity.Null || !_entityManager.HasComponent<EnemyCombatState>(_entity))
                return;

            var combat = _entityManager.GetComponentData<EnemyCombatState>(_entity);
            combat.NeedsCombatResult = false;
            _entityManager.SetComponentData(_entity, combat);
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
            _disposables.Dispose();
        }
    }
}