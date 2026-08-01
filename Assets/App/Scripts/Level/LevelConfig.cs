using System;
using System.Collections.Generic;
using App.Enemy.Wave;
using UnityEngine;

namespace App.Level
{
    [Serializable]
    public class LevelInfor
    {
        [SerializeField] int _levelId;
        [SerializeField] List<WaveEntry> _waveEntries;
        [SerializeField] string _sceneName;

        public int LevelId => _levelId;
        public List<WaveEntry> WaveEntries => _waveEntries;
        public string SceneName => _sceneName;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ZombiesWar/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] List<LevelInfor> _levels;

        public List<LevelInfor> Levels => _levels;
    }
}
