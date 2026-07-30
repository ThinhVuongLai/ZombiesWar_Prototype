using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombiesWar.Weapon
{
    [Serializable]
    public class WeaponConfig
    {
        [SerializeField] int _weaponId;
        [SerializeField] int _bulletId;
        [SerializeField] float _attackCooldown = 0.5f;

        public int WeaponId => _weaponId;
        public int BulletId => _bulletId;
        public float AttackCooldown => _attackCooldown;
    }

    [CreateAssetMenu(fileName = "WeaponConfigRegistry", menuName = "ZombiesWar/Weapon Config Registry")]
    public class WeaponConfigRegistry : ScriptableObject
    {
        [SerializeField] WeaponConfig[] _configs;

        readonly Dictionary<int, WeaponConfig> _lookup = new();

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

        public WeaponConfig GetConfig(int weaponId)
        {
            _lookup.TryGetValue(weaponId, out var config);
            return config;
        }
    }
}
