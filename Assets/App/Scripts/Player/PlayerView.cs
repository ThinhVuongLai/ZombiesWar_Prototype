using System;
using App.Core.Services;
using App.HealthBar;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Weapon;

namespace App.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerView : MonoBehaviour, IPlayerView, IPlayerTargetProvider
    {
        [SerializeField] WeaponConfigRegistry _weaponConfigRegistry;
        [SerializeField] BulletConfigRegistry _bulletConfigRegistry;
        [SerializeField] HealthBarConfig _healthBarConfig;

        CharacterController _characterController;
        PlayerPresenter _presenter;

        public bool IsGrounded => _characterController.isGrounded;
        public Transform Transform => transform;
        public Transform PlayerTransform => transform;

        public bool IsAlive => _presenter?.IsAlive ?? true;

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            var input = ServiceLocator.Resolve<IPlayerInputProvider>();
            var eventBus = ServiceLocator.Resolve<Core.EventBus.IEventBus>();
            _presenter = new PlayerPresenter(this, input, eventBus, _weaponConfigRegistry, _bulletConfigRegistry);
        }

        void OnDestroy()
        {
            _presenter?.Dispose();
        }

        public void Move(Vector3 motion)
        {
            _characterController.Move(motion);
        }

        public void FaceDirection(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }

        public IHealthBarView CreateHealthBar()
        {
            if (_healthBarConfig == null) return null;

            var go = new GameObject("HealthBar");
            go.transform.SetParent(transform, false);
            var view = go.AddComponent<HealthBarView>();
            view.Initialize(_healthBarConfig, transform);
            return view;
        }
    }
}