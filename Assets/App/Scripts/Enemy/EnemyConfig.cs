using System;
using System.Collections.Generic;
using UnityEngine;

namespace App.Enemy
{
    [Serializable]
    public class EnemyInfor
    {
        public int EnemyId;
        public GameObject EnemyPrefab;

        [Header("Animations")]
        public string IdleAnimationName;
        public string MoveAnimationName;
        public string AttackAnimationName;
        public string DeadAnimationName;
    }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "ZombiesWar/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] List<EnemyInfor> _enemies = new();

        Dictionary<int, EnemyInfor> _lookup;

        public EnemyInfor GetEnemyInfor(int enemyId)
        {
            if (_lookup == null)
                BuildLookup();
            _lookup.TryGetValue(enemyId, out var info);
            return info;
        }

        void OnEnable()
        {
            BuildLookup();
        }

        void BuildLookup()
        {
            _lookup = new Dictionary<int, EnemyInfor>();
            if (_enemies == null) return;

            foreach (var e in _enemies)
            {
                if (e != null && !_lookup.ContainsKey(e.EnemyId))
                    _lookup[e.EnemyId] = e;
            }
        }
    }
}
