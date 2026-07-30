using System;
using System.Collections.Generic;
using App.Combat.Attack;
using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace ZombiesWar.Weapon
{
    [Serializable]
    public abstract class WeaponBase
    {
        [SerializeField] protected int _weaponId;
        [SerializeField] protected WeaponType _weaponType;
        [SerializeField] protected float _attackCooldown = 0.5f;
        [SerializeField] protected float _damage = 10f;
        [SerializeField] protected float _attackRange = 2f;

        public int WeaponId => _weaponId;
        public WeaponType WeaponType => _weaponType;
        public float AttackCooldown => _attackCooldown;
        public float Damage => _damage;
        public float AttackRange => _attackRange;
    }

    [Serializable]
    public class MeleeWeaponConfig : WeaponBase
    {
    }

    [Serializable]
    public class RangeWeaponConfig : WeaponBase
    {
        [SerializeField] int _bulletId;

        public int BulletId => _bulletId;
    }

    [Serializable]
    public class ThrowWeaponConfig : WeaponBase, IThrowConfig
    {
        [SerializeField] float _throwAngle = 45f;
        [SerializeField] float _minThrowForce = 5f;
        [SerializeField] float _maxThrowForce = 20f;
        [SerializeField] float _throwSpeed = 10f;
        [SerializeField] ThrowActionType _actionType = ThrowActionType.Explosion;
        [SerializeField] float _actionRadius = 3f;
        [SerializeField] float _objectLifespan = 5f;
        [SerializeField] float _gravityScale = 1f;
        [SerializeField] GameObject _objectPrefab;

        public float ThrowAngle => _throwAngle;
        public float MinThrowForce => _minThrowForce;
        public float MaxThrowForce => _maxThrowForce;
        public float ThrowSpeed => _throwSpeed;
        public ThrowActionType ActionType => _actionType;
        public float ActionRadius => _actionRadius;
        public float ObjectLifespan => _objectLifespan;
        public float GravityScale => _gravityScale;
        public GameObject ObjectPrefab => _objectPrefab;
    }

    [CreateAssetMenu(fileName = "WeaponConfigRegistry", menuName = "ZombiesWar/Weapon Config Registry")]
    public class WeaponConfigRegistry : ScriptableObject
    {
        [SerializeReference]
        [SerializeField] WeaponBase[] _configs;

        readonly Dictionary<int, WeaponBase> _lookup = new();

        void OnEnable()
        {
            _lookup.Clear();
            if (_configs == null) return;
            foreach (var c in _configs)
            {
                if (c != null)
                    _lookup[c.WeaponId] = c;
            }
        }

        public WeaponBase GetConfig(int weaponId)
        {
            _lookup.TryGetValue(weaponId, out var config);
            return config;
        }
    }
}
