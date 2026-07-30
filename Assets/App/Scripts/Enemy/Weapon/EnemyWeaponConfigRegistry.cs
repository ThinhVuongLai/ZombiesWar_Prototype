using System;
using System.Collections.Generic;
using UnityEngine;

namespace App.Enemy.Weapon
{
    [Serializable]
    public class EnemyWeaponConfig
    {
        [SerializeField] EnemyAttackType _attackType;
        [SerializeField] int _bulletId = -1;
        [SerializeField] float _attackDamage = 10f;
        [SerializeField] float _attackRange = 2f;
        [SerializeField] float _attackCooldown = 1.5f;

        public EnemyAttackType AttackType => _attackType;
        public int BulletId => _bulletId;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
    }

    [CreateAssetMenu(fileName = "EnemyWeaponConfigRegistry", menuName = "ZombiesWar/Enemy Weapon Config Registry")]
    public class EnemyWeaponConfigRegistry : ScriptableObject
    {
        [SerializeField] EnemyWeaponConfig[] _configs;

        readonly Dictionary<EnemyAttackType, EnemyWeaponConfig> _lookup = new();

        void OnEnable()
        {
            _lookup.Clear();
            if (_configs == null) return;
            foreach (var c in _configs)
            {
                if (c != null)
                    _lookup[c.AttackType] = c;
            }
        }

        public EnemyWeaponConfig GetConfig(EnemyAttackType type)
        {
            _lookup.TryGetValue(type, out var config);
            return config;
        }
    }
}
