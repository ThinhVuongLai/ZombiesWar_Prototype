using System;
using System.Collections.Generic;
using App.Core.EventBus;
using App.Player.Combat;
using App.Player.States;
using R3;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Bullet.ECS;
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

        Entity _weaponTargetEntity;
        bool _weaponEntityResolved;

        BulletConfig _currentBulletConfig;
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

            if (_currentBulletConfig != null && HasCombatTarget)
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
            if (!_weaponEntityResolved)
            {
                var query = _entityManager.CreateEntityQuery(typeof(PlayerWeaponTargetData));
                if (query.CalculateEntityCount() > 0)
                {
                    _weaponTargetEntity = query.GetSingletonEntity();
                    _weaponEntityResolved = true;
                }
            }

            if (_weaponTargetEntity == Entity.Null) return;

            var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
            _combatModel.HasTarget.Value = data.HasTarget;
            _combatModel.TargetDirection.Value = data.TargetDirection;
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
                return;
            }

            _currentBulletConfig = _bulletConfigRegistry?.GetConfig(weaponConfig.BulletId);
            _combatModel.AttackCooldown.Value = weaponConfig.AttackCooldown;
        }

        public void TryAttack()
        {
            if (_currentBulletConfig == null) return;

            if (Time.time - _lastAttackTime < _combatModel.AttackCooldown.Value) return;

            if (!_weaponEntityResolved || _weaponTargetEntity == Entity.Null) return;

            var data = _entityManager.GetComponentData<PlayerWeaponTargetData>(_weaponTargetEntity);
            if (!data.HasTarget) return;

            _lastAttackTime = Time.time;

            var firePos = (float3)_view.Transform.position + new float3(0, 1.5f, 0);

            BulletSpawner.SpawnBullet(_currentBulletConfig, firePos, data.CurrentTargetEntity);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}