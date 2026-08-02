using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Audio
{
    [CreateAssetMenu(fileName = "MusicConfig", menuName = "ZombiesWar/Music Config")]
    public class MusicConfig : ScriptableObject
    {
        [SerializeField] MusicInfor[] _musicList;

        readonly Dictionary<Music, MusicInfor> _lookup = new();

        void OnEnable()
        {
            _lookup.Clear();
            if (_musicList == null) return;
            foreach (var musicInfor in _musicList)
            {
                if (musicInfor != null)
                    _lookup[musicInfor.Music] = musicInfor;
            }
        }

        public MusicInfor GetMusicInfor(Music music)
        {
            _lookup.TryGetValue(music, out var musicInfor);
            return musicInfor;
        }
    }

    [System.Serializable]
    public class MusicInfor
    {
        [SerializeField] Music _music;
        [SerializeField] AudioClip _audioClip;
        [SerializeField][Range(0f, 1f)] float _volume = 1f;

        public Music Music => _music;
        public AudioClip AudioClip => _audioClip;
        public float Volume => _volume;
    }
}