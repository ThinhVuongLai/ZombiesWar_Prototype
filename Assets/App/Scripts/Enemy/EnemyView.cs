using System;
using App.Core;
using App.Core.Services;
using App.HealthBar;
using UnityEngine;
using UnityEngine.AI;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy
{
    public struct EnemyViewConfig
    {
        public readonly WeaponType AttackType;
        public readonly float MoveSpeed;
        public readonly float Health;
        public readonly float DetectionRange;
        public readonly string IdleAnimationName;
        public readonly string MoveAnimationName;
        public readonly string AttackAnimationName;
        public readonly string DeadAnimationName;

        public EnemyViewConfig(WeaponType attackType, float moveSpeed, float health,
            float detectionRange,
            string idleAnimationName = null, string moveAnimationName = null,
            string attackAnimationName = null, string deadAnimationName = null)
        {
            AttackType = attackType;
            MoveSpeed = moveSpeed;
            Health = health;
            DetectionRange = detectionRange;
            IdleAnimationName = idleAnimationName;
            MoveAnimationName = moveAnimationName;
            AttackAnimationName = attackAnimationName;
            DeadAnimationName = deadAnimationName;
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyView : MonoBehaviour, IEnemyView, IDamageable
    {
        [SerializeField] WeaponType _attackType = WeaponType.Melee;

        [Header("Stats")]
        [SerializeField] float _moveSpeed = 3.5f;
        [SerializeField] float _health = 100f;
        [SerializeField] float _detectionRange = 12f;

        [Header("Animation")]
        [SerializeField] Animator _animator;

        NavMeshAgent _agent;
        EnemyViewConfig _config;

        public EnemyViewConfig Config => _config;

        public Transform Transform => transform;
        public bool HasPath => _agent != null && _agent.hasPath;
        public float RemainingDistance => _agent != null ? _agent.remainingDistance : 0f;

        public Action OnDestroyed { get; set; }
        public Action<float> TakeExternalDamage { get; set; }

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _config = new EnemyViewConfig(_attackType, _moveSpeed, _health, _detectionRange);
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
            var cm = ServiceLocator.Resolve<ConfigManager>();
            if (cm.EnemyHealthBarConfig == null) return null;

            var go = new GameObject("HealthBar");
            go.transform.SetParent(transform, false);
            var view = go.AddComponent<HealthBarView>();
            view.Initialize(cm.EnemyHealthBarConfig, transform);
            return view;
        }
    }
}