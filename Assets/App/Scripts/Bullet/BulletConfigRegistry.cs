using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombiesWar.Bullet
{
    [Serializable]
    public class BulletConfig
    {
        [SerializeField] int _bulletId;
        [SerializeField] BulletMovementType _movementType;
        [SerializeField] float _speed = 10f;
        [SerializeField] float _range = 15f;
        [SerializeField] GameObject _visualPrefab;

        public int BulletId => _bulletId;
        public BulletMovementType MovementType => _movementType;
        public float Speed => _speed;
        public float Range => _range;
        public GameObject VisualPrefab => _visualPrefab;
    }

    [CreateAssetMenu(fileName = "BulletConfigRegistry", menuName = "ZombiesWar/Bullet Config Registry")]
    public class BulletConfigRegistry : ScriptableObject
    {
        [SerializeField] BulletConfig[] _configs;

        readonly Dictionary<int, BulletConfig> _lookup = new();

        void OnEnable()
        {
            _lookup.Clear();
            if (_configs == null) return;
            foreach (var c in _configs)
            {
                if (c != null)
                    _lookup[c.BulletId] = c;
            }
        }

        public BulletConfig GetConfig(int bulletId)
        {
            _lookup.TryGetValue(bulletId, out var config);
            return config;
        }
    }
}
