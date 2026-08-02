using System;
using App.Core;
using App.Core.Services;
using App.HealthBar;
using DG.Tweening;
using MagicTile.Pool;
using UnityEngine;
using UnityEngine.AI;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy
{
    public struct EnemyViewConfig
    {
        public readonly float MoveSpeed;
        public readonly float DetectionRange;
        public readonly string IdleAnimationName;
        public readonly string MoveAnimationName;
        public readonly string AttackAnimationName;
        public readonly string DeadAnimationName;

        public EnemyViewConfig(float moveSpeed,
            float detectionRange,
            string idleAnimationName = null, string moveAnimationName = null,
            string attackAnimationName = null, string deadAnimationName = null)
        {
            MoveSpeed = moveSpeed;
            DetectionRange = detectionRange;
            IdleAnimationName = idleAnimationName;
            MoveAnimationName = moveAnimationName;
            AttackAnimationName = attackAnimationName;
            DeadAnimationName = deadAnimationName;
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyView : MonoBehaviour, IEnemyView, IDamageable, IPoolable
    {
        [Header("Stats")]
        [SerializeField] float _moveSpeed = 3.5f;
        [SerializeField] float _detectionRange = 12f;

        [Header("Animation")]
        [SerializeField] Animator _animator;

        NavMeshAgent _agent;
        EnemyViewConfig _config;
        SkinnedMeshRenderer[] _meshRenderers;
        MaterialPropertyBlock _materialPropertyBlock;
        Tween _damageFlashTween;
        Color _originalBaseColor;
        string _colorPropertyName = "_BaseColor";

        public EnemyViewConfig Config => _config;

        public Transform Transform => transform;
        public bool HasPath => _agent != null && _agent.hasPath;
        public float RemainingDistance => _agent != null ? _agent.remainingDistance : 0f;

        public Action OnDestroyed { get; set; }
        public Action<float> TakeExternalDamage { get; set; }

        public IDisposable CurrentPresenter { get; set; }

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _config = new EnemyViewConfig(_moveSpeed, _detectionRange);
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
        }

        public void SetConfig(EnemyViewConfig config)
        {
            _config = config;
        }

        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

        public void SetDestination(Vector3 target)
        {
            if (_agent != null && _agent.isActiveAndEnabled)
                _agent.SetDestination(target);
        }

        public void StopMovement()
        {
            if (_agent != null && _agent.isActiveAndEnabled)
                _agent.ResetPath();
        }

        public void SetAgentEnabled(bool enabled)
        {
            if (_agent != null)
                _agent.enabled = enabled;
        }

        public void PlayAnimation(string animationName, int layerIndex = 0)
        {
            if (_animator != null && !string.IsNullOrEmpty(animationName))
                _animator.Play(animationName, layerIndex);
        }

        void IDamageable.TakeDamage(float damage)
        {
            TakeExternalDamage?.Invoke(damage);
        }

        public IHealthBarView CreateHealthBar()
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            if (configManager.EnemyHealthBarConfig == null) return null;

            var healthBarObject = new GameObject("HealthBar");
            healthBarObject.transform.SetParent(transform, false);
            var view = healthBarObject.AddComponent<HealthBarView>();
            view.Initialize(configManager.EnemyHealthBarConfig, transform);
            return view;
        }

        public void SetDissolveAmount(float amount)
        {
            if (_meshRenderers == null) return;

            if (_materialPropertyBlock == null)
                _materialPropertyBlock = new MaterialPropertyBlock();

            _materialPropertyBlock.SetFloat("_DissolveAmount", amount);
            ApplyPropertyBlock();
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

            _damageFlashTween.Play();
        }

        void ApplyPropertyBlock()
        {
            if (_materialPropertyBlock == null) return;
            for (int i = 0; i < _meshRenderers.Length; i++)
                _meshRenderers[i].SetPropertyBlock(_materialPropertyBlock);
        }

        void IPoolable.OnGetFromPool()
        {
            SetDissolveAmount(0f);
        }

        void IPoolable.OnReleaseToPool()
        {
            SetDissolveAmount(0f);
        }
    }
}