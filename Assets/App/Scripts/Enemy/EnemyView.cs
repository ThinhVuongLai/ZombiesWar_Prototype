using System;
using App.Core;
using App.Core.Services;
using App.HealthBar;
using UnityEngine;
using UnityEngine.AI;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy
{
    public readonly struct EnemyViewConfig
    {
        public readonly WeaponType AttackType;
        public readonly float MoveSpeed;
        public readonly float Health;
        public readonly float DetectionRange;

        public EnemyViewConfig(WeaponType attackType, float moveSpeed, float health,
            float detectionRange)
        {
            AttackType = attackType;
            MoveSpeed = moveSpeed;
            Health = health;
            DetectionRange = detectionRange;
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