using System;
using App.Core;
using App.Core.Services;
using App.HealthBar;
using DG.Tweening;
using UnityEngine;
using ZombiesWar.Weapon;

namespace App.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerView : MonoBehaviour, IPlayerView, IPlayerTargetProvider
    {
        [SerializeField] Animator _animator;
        [SerializeField] Transform _rangeContain;
        [SerializeField] Transform _meleeContain;
        [SerializeField] Transform _thrownContain;

        CharacterController _characterController;
        PlayerPresenter _presenter;
        SkinnedMeshRenderer[] _meshRenderers;
        MaterialPropertyBlock _materialPropertyBlock;
        Tween _damageFlashTween;
        Color _originalBaseColor;
        string _colorPropertyName = "_BaseColor";
        GameObject _currentWeaponModel;

        public bool IsGrounded => _characterController.isGrounded;
        public Transform Transform => transform;
        public Transform PlayerTransform => transform;

        public bool IsAlive => _presenter?.IsAlive ?? true;

        void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            if (_meshRenderers is { Length: > 0 })
            {
                var sharedMaterial = _meshRenderers[0].sharedMaterial;
                if (sharedMaterial != null)
                {
                    _colorPropertyName = sharedMaterial.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                    _originalBaseColor = sharedMaterial.GetColor(_colorPropertyName);
                }
                else
                {
                    _originalBaseColor = Color.white;
                }
            }
            else
            {
                _originalBaseColor = Color.white;
            }

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

        public void PlayDamageFlash(Color flashColor, float duration)
        {
            if (_meshRenderers is not { Length: > 0 }) return;

            if (_materialPropertyBlock == null)
                _materialPropertyBlock = new MaterialPropertyBlock();

            _damageFlashTween?.Kill();

            var currentColor = _originalBaseColor;
            var halfDuration = duration * 0.5f;

            var sequence = DOTween.Sequence();

            sequence.Append(DOTween.To(
                () => currentColor,
                value =>
                {
                    currentColor = value;
                    _materialPropertyBlock.SetColor(_colorPropertyName, value);
                    ApplyPropertyBlock();
                },
                flashColor,
                halfDuration));

            sequence.Append(DOTween.To(
                () => currentColor,
                value =>
                {
                    currentColor = value;
                    _materialPropertyBlock.SetColor(_colorPropertyName, value);
                    ApplyPropertyBlock();
                },
                _originalBaseColor,
                halfDuration));

            _damageFlashTween = sequence;
        }

        public PlayerWeaponItem SetWeaponModel(WeaponBase weaponConfig)
        {
            if (_currentWeaponModel != null)
            {
                Destroy(_currentWeaponModel);
                _currentWeaponModel = null;
            }

            if (weaponConfig?.WeaponPrefab == null) return null;

            var contain = weaponConfig.WeaponType switch
            {
                WeaponType.Melee => _meleeContain,
                WeaponType.Range => _rangeContain,
                WeaponType.Throwing => _thrownContain,
                _ => null,
            };
            if (contain == null) return null;

            _currentWeaponModel = Instantiate(weaponConfig.WeaponPrefab, contain);
            _currentWeaponModel.transform.localPosition = Vector3.zero;
            _currentWeaponModel.transform.localEulerAngles = Vector3.zero;

            PlayerWeaponItem weaponItem = _currentWeaponModel.GetComponent<PlayerWeaponItem>();
            weaponItem?.Init();

            return weaponItem;
        }

        void ApplyPropertyBlock()
        {
            if (_materialPropertyBlock == null) return;
            for (int i = 0; i < _meshRenderers.Length; i++)
                _meshRenderers[i].SetPropertyBlock(_materialPropertyBlock);
        }
    }
}