using System;
using System.Collections.Generic;
using App.Combat.Attack;
using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using App.Enemy.States;
using App.Enemy.Wave;
using App.Enemy.Weapon;
using App.HealthBar;
using DG.Tweening;
using MagicTile.Pool;
using R3;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using App.Audio;

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
        readonly EnemyWeaponConfig _weaponConfig;
        readonly EnemyMeleeWeaponConfig _meleeWeaponConfig;
        readonly string _idleAnimation;
        readonly string _moveAnimation;
        readonly string _attackAnimation;
        readonly string _deadAnimation;

        IEnemyState _currentState;
        Entity _entity;
        HealthBarPresenter _healthBarPresenter;
        bool _isDead;
        float _dissolveDuration;
        float _previousHealth;

        public IEnemyView View => _view;
        public IPlayerTargetProvider PlayerTarget => _playerTarget;
        public float AttackDamage { get; }
        public float AttackRange { get; }

        float _lastAttackTime;

        float _attackTimer;
        bool _isInAttackDuration;
        bool _damageAppliedThisAttack;
        float _currentAttackDuration;
        float _currentTakeDamageTime;
        Vector2 _currentHitZoneSize;

        public bool IsInAttackDuration => _isInAttackDuration;

        public EnemyDetectionState CachedDetectionState { get; private set; }

        public EnemyPresenter(IEnemyView view, in EnemyViewConfig config,
            EnemyWeaponConfig weaponConfig, AttackStrategyRegistry registry,
            IPlayerTargetProvider playerTarget, float health, float dissolveDuration = 1.5f)
        {
            _view = view;
            _playerTarget = playerTarget;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _attackRegistry = registry;
            _weaponConfig = weaponConfig;
            _meleeWeaponConfig = weaponConfig as EnemyMeleeWeaponConfig;
            _idleAnimation = config.IdleAnimationName;
            _moveAnimation = config.MoveAnimationName;
            _attackAnimation = config.AttackAnimationName;
            _deadAnimation = config.DeadAnimationName;
            _dissolveDuration = dissolveDuration;

            _model = new EnemyModel();
            AttackDamage = weaponConfig?.Damage ?? 10f;
            AttackRange = weaponConfig?.AttackRange ?? 2f;

            _model.MoveSpeed.Value = config.MoveSpeed;
            _model.Health.Value = health;
            _model.MaximumHealth.Value = health;
            _previousHealth = health;
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
                _healthBarPresenter = new HealthBarPresenter(healthBarView, _model.Health, health);
            }

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Subscribe(_ => OnLateUpdate())
                .AddTo(_disposables);
        }

        public void SetEntity(Entity entity)
        {
            _entity = entity;

            var entityManager = _entityManager;

            if (entityManager.HasComponent<EnemyStats>(entity))
            {
                entityManager.SetComponentData(entity, new EnemyStats
                {
                    MoveSpeed = _model.MoveSpeed.Value,
                    AttackDamage = _model.AttackDamage.Value,
                    AttackRange = AttackRange,
                    DetectionRange = _model.DetectionRange.Value,
                    AttackCooldown = _model.AttackCooldown.Value,
                });
            }

            if (entityManager.HasComponent<EnemyHealth>(entity))
            {
                entityManager.SetComponentData(entity, new EnemyHealth { Value = _model.Health.Value });
            }

            if (entityManager.HasComponent<EnemyCombatState>(entity))
            {
                entityManager.SetComponentData(entity, new EnemyCombatState
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
                var localTransform = _entityManager.GetComponentData<LocalTransform>(_entity);
                localTransform.Position = (float3)_view.Transform.position;
                _entityManager.SetComponentData(_entity, localTransform);
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

                if (health.Value < _previousHealth)
                {
                    var globalData = ServiceLocator.Resolve<ConfigManager>().GlobalData;
                    if (globalData != null)
                        _view.PlayDamageFlash(globalData.DamageFlashColor, globalData.DamageFlashDuration);
                }
                _previousHealth = health.Value;

                if (health.Value <= 0f && _model.CurrentState.Value != EnemyStateType.Die)
                {
                    _isDead = true;
                    ServiceLocator.Resolve<IEventBus>().Publish(new EnemyDefeatedMessage());
                    TransitionTo(EnemyStateType.Die);
                    return;
                }
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
                case EnemyStateType.Die:
                    {
                        _view.PlayAnimation(_deadAnimation);
                        ServiceLocator.Resolve<AudioManager>()?.RunSfx(SFX.ZombieDead);
                    }
                    break;
            }

            _currentState.Enter(this);

            if (newState == EnemyStateType.Die)
                StartDissolveEffect();
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            if (_entity == Entity.Null || !_entityManager.HasComponent<EnemyHealth>(_entity))
                return;

            var health = _entityManager.GetComponentData<EnemyHealth>(_entity);
            health.Value -= damage;
            _entityManager.SetComponentData(_entity, health);

            var globalData = ServiceLocator.Resolve<ConfigManager>().GlobalData;
            if (globalData != null)
                _view.PlayDamageFlash(globalData.DamageFlashColor, globalData.DamageFlashDuration);

            UnityEngine.Debug.Log($"Enemy Get Damage, {health}");

            if (health.Value <= 0f)
            {
                _isDead = true;
                ServiceLocator.Resolve<IEventBus>().Publish(new EnemyDefeatedMessage());
                TransitionTo(EnemyStateType.Die);

                UnityEngine.Debug.Log($"Change to Die");
            }
        }

        public bool TryAttack(float currentTime)
        {
            if (_isInAttackDuration) return false;

            var cooldown = _model.AttackCooldown.Value;
            if (currentTime - _lastAttackTime < cooldown)
                return false;

            _lastAttackTime = currentTime;

            if (_weaponConfig.WeaponType == WeaponType.Melee && _meleeWeaponConfig != null)
            {
                _isInAttackDuration = true;
                _attackTimer = 0f;
                _damageAppliedThisAttack = false;
                _currentAttackDuration = _meleeWeaponConfig.AttackDuration;
                _currentTakeDamageTime = _meleeWeaponConfig.TakeDamageTime;
                _currentHitZoneSize = _meleeWeaponConfig.HitZoneSize;
                _view.PlayAnimation(_attackAnimation);
                ServiceLocator.Resolve<AudioManager>()?.RunSfx(SFX.ZombieAttack);

                return true;
            }

            ExecuteAttack();
            _view.PlayAnimation(_attackAnimation);
            return true;
        }

        void ApplyMeleeDamage()
        {
            if (_isDead) return;

            var attackerPosition = _view.Transform.position;
            var attackerForward = _view.Transform.forward;
            var targetPosition = _playerTarget.PlayerTransform.position;

            if (!MeleeAttackStrategy.IsTargetInHitZone(attackerPosition, attackerForward, targetPosition, _currentHitZoneSize))
                return;

            var healthAccessor = new PlayerHealthAccessor();
            if (!healthAccessor.TryApplyDamage(PlayerTargetECSUpdater.PlayerEntity, AttackDamage))
            {
                ServiceLocator.Resolve<IEventBus>()
                    .Publish(new EnemyDealtDamageMessage(AttackDamage, _weaponConfig.WeaponType));
            }
        }

        public void ProcessAttackDuration()
        {
            if (!_isInAttackDuration) return;

            _attackTimer += Time.deltaTime;
            if (!_damageAppliedThisAttack && _attackTimer >= _currentAttackDuration * _currentTakeDamageTime)
            {
                ApplyMeleeDamage();
                _damageAppliedThisAttack = true;
            }
            if (_attackTimer >= _currentAttackDuration)
            {
                _isInAttackDuration = false;
            }
        }

        public void DestroyECSCombatState()
        {
            if (_entity != Entity.Null && _entityManager.Exists(_entity))
                _entityManager.DestroyEntity(_entity);
        }

        void StartDissolveEffect()
        {
            float dissolveAmount = 0f;
            DOTween.To(() => dissolveAmount, value =>
            {
                dissolveAmount = value;
                _view.SetDissolveAmount(value);
            }, 1f, _dissolveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                var poolService = ServiceLocator.Resolve<PoolService>();
                var gameObject = (_view as MonoBehaviour)?.gameObject;
                if (gameObject != null)
                    poolService.Release(gameObject);
                Dispose();
            });
        }

        public void ExecuteAttack()
        {
            var strategy = _attackRegistry.Get(_weaponConfig.WeaponType);
            if (strategy == null) return;

            strategy.Execute(
                _view.Transform.position, _view.Transform,
                PlayerTargetECSUpdater.PlayerEntity,
                _playerTarget.PlayerTransform.position,
                AttackDamage,
                new PlayerHealthAccessor(),
                faceTarget: true,
                fallbackDamageDealer: damage =>
                    ServiceLocator.Resolve<IEventBus>()
                        .Publish(new EnemyDealtDamageMessage(damage, _weaponConfig.WeaponType)));
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
