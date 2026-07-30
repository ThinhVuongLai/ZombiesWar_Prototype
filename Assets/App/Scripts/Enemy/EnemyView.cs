using System;
using UnityEngine;
using UnityEngine.AI;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy
{
    public readonly struct EnemyViewConfig
    {
        public readonly EnemyAttackType AttackType;
        public readonly float MoveSpeed;
        public readonly float Health;
        public readonly float DetectionRange;

        public EnemyViewConfig(EnemyAttackType attackType, float moveSpeed, float health,
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
        [SerializeField] EnemyAttackType _attackType = EnemyAttackType.Melee;

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
    }
}