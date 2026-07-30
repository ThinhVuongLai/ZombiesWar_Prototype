using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombiesWar.ThrowingWeapon
{
    [Serializable]
    public class ThrowWeaponConfig
    {
        [SerializeField] int _weaponId;
        [SerializeField] float _throwAngle = 45f;
        [SerializeField] float _minThrowForce = 5f;
        [SerializeField] float _maxThrowForce = 20f;
        [SerializeField] float _throwSpeed = 10f;
        [SerializeField] ThrowActionType _actionType = ThrowActionType.Explosion;
        [SerializeField] float _actionRadius = 3f;
        [SerializeField] float _actionDamage = 20f;
        [SerializeField] float _objectLifespan = 5f;
        [SerializeField] float _gravityScale = 1f;
        [SerializeField] GameObject _objectPrefab;

        public int WeaponId => _weaponId;
        public float ThrowAngle => _throwAngle;
        public float MinThrowForce => _minThrowForce;
        public float MaxThrowForce => _maxThrowForce;
        public float ThrowSpeed => _throwSpeed;
        public ThrowActionType ActionType => _actionType;
        public float ActionRadius => _actionRadius;
        public float ActionDamage => _actionDamage;
        public float ObjectLifespan => _objectLifespan;
        public float GravityScale => _gravityScale;
        public GameObject ObjectPrefab => _objectPrefab;
    }

    [CreateAssetMenu(fileName = "ThrowWeaponConfigRegistry", menuName = "ZombiesWar/Throw Weapon Config Registry")]
    public class ThrowWeaponConfigRegistry : ScriptableObject
    {
        [SerializeField] ThrowWeaponConfig[] _configs;

        readonly Dictionary<int, ThrowWeaponConfig> _lookup = new();

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

        public ThrowWeaponConfig GetConfig(int weaponId)
        {
            _lookup.TryGetValue(weaponId, out var config);
            return config;
        }
    }
}
