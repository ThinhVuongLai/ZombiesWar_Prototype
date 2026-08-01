using System;
using App.Core;
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
        [SerializeField] Animator _animator;

        CharacterController _characterController;
        PlayerPresenter _presenter;

        public bool IsGrounded => _characterController.isGrounded;
        public Transform Transform => transform;
        public Transform PlayerTransform => transform;

        public bool IsAlive => _presenter?.IsAlive ?? true;

        void Start()
        {
            _characterController = GetComponent<CharacterController>();
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            var input = ServiceLocator.Resolve<IPlayerInputProvider>();
            var eventBus = ServiceLocator.Resolve<Core.EventBus.IEventBus>();
            _presenter = new PlayerPresenter(this, configManager.PlayerConfig, input, eventBus, configManager.WeaponConfigRegistry, configManager.BulletConfigRegistry);
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

        public void PlayMoveAnimation(string animationName, int layerIndex)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName)) return;
            _animator.Play(animationName, layerIndex);
        }

        public void PlayAttackAnimation(string animationName, int layerIndex)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName)) return;
            _animator.Play(animationName, layerIndex);
        }

        public IHealthBarView CreateHealthBar()
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            if (configManager.HealthBarConfig == null) return null;

            var healthBarObject = new GameObject("HealthBar");
            healthBarObject.transform.SetParent(transform, false);
            var view = healthBarObject.AddComponent<HealthBarView>();
            view.Initialize(configManager.HealthBarConfig, transform);
            return view;
        }
    }
}