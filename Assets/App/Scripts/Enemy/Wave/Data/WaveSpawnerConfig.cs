using System;
using UnityEngine;

namespace App.Enemy.Wave
{
    [Serializable]
    public class WaveEntry
    {
        public GameObject EnemyPrefab;
        public int EnemyCount = 3;
        public float SpawnRadius = 30f;
        public float IntervalBeforeNext = 10f;
    }

    [CreateAssetMenu(fileName = "WaveSpawnerConfig", menuName = "ZombiesWar/Wave Spawner Config")]
    public class WaveSpawnerConfig : ScriptableObject
    {
        [SerializeField] WaveEntry[] _waves;

        public WaveEntry[] Waves => _waves;
        public int WaveCount => _waves?.Length ?? 0;

        public WaveEntry GetWave(int index)
        {
            if (_waves == null || index < 0 || index >= _waves.Length)
                return null;
            return _waves[index];
        }
    }
}
