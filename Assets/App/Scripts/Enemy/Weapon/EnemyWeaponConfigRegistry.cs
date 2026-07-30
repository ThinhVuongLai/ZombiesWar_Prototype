using System;
using System.Collections.Generic;
using App.Combat.Attack;
using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy.Weapon
{
    [Serializable]
    public abstract class EnemyWeaponConfig
    {
        [SerializeField] protected WeaponType _weaponType;
        [SerializeField] protected float _damage = 10f;
        [SerializeField] protected float _attackRange = 2f;
        [SerializeField] protected float _attackCooldown = 1.5f;

        public WeaponType WeaponType => _weaponType;
        public float Damage => _damage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
    }

    [Serializable]
    public class EnemyMeleeWeaponConfig : EnemyWeaponConfig
    {
    }

    [Serializable]
    public class EnemyRangedWeaponConfig : EnemyWeaponConfig
    {
        [SerializeField] int _bulletId = -1;

        public int BulletId => _bulletId;
    }

    [Serializable]
    public class EnemyThrowWeaponConfig : EnemyWeaponConfig, IThrowConfig
    {
        [SerializeField] float _throwAngle = 45f;
        [SerializeField] float _minThrowForce = 5f;
        [SerializeField] float _maxThrowForce = 20f;
        [SerializeField] ThrowActionType _actionType = ThrowActionType.Explosion;
        [SerializeField] float _actionRadius = 3f;
        [SerializeField] float _objectLifespan = 5f;
        [SerializeField] float _gravityScale = 1f;
        [SerializeField] GameObject _objectPrefab;

        public float ThrowAngle => _throwAngle;
        public float MinThrowForce => _minThrowForce;
        public float MaxThrowForce => _maxThrowForce;
        public ThrowActionType ActionType => _actionType;
        public float ActionRadius => _actionRadius;
        public float ObjectLifespan => _objectLifespan;
        public float GravityScale => _gravityScale;
        public GameObject ObjectPrefab => _objectPrefab;
    }

    [CreateAssetMenu(fileName = "EnemyWeaponConfigRegistry", menuName = "ZombiesWar/Enemy Weapon Config Registry")]
    public class EnemyWeaponConfigRegistry : ScriptableObject
    {
        [SerializeReference]
        [SerializeField] EnemyWeaponConfig[] _configs;

        readonly Dictionary<WeaponType, EnemyWeaponConfig> _lookup = new();

        void OnEnable()
        {
            _lookup.Clear();
            if (_configs == null) return;
            foreach (var c in _configs)
            {
                if (c != null)
                    _lookup[c.WeaponType] = c;
            }
        }

        public EnemyWeaponConfig GetConfig(WeaponType type)
        {
            _lookup.TryGetValue(type, out var config);
            return config;
        }
    }
}
