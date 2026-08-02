using System;
using App.Core;
using App.Core.Services;
using MagicTile.Pool;
using R3;
using UnityEngine;

namespace App.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioSource _musicSource;
        [SerializeField] GameObject _sfxSourcePrefab;

        MusicConfig _musicConfig;
        SfxConfig _sfxConfig;
        PoolService _poolService;
        CompositeDisposable _disposables;

        void Awake()
        {
            ServiceLocator.Register(this);
        }

        void Start()
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            _musicConfig = configManager.MusicConfig;
            _sfxConfig = configManager.SfxConfig;
            _poolService = ServiceLocator.Resolve<PoolService>();
            _disposables = new CompositeDisposable();
        }

        public void RunMusic(Music music)
        {
            var musicInfor = _musicConfig?.GetMusicInfor(music);
            if (musicInfor == null || musicInfor.AudioClip == null)
            {
                Debug.LogWarning($"[AudioManager] Khong tim thay MusicInfor cho '{music}' hoac AudioClip null.");
                return;
            }

            _musicSource.clip = musicInfor.AudioClip;
            _musicSource.volume = musicInfor.Volume;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void RunSfx(SFX sfx)
        {
            var sfxInfor = _sfxConfig?.GetSfxInfor(sfx);
            if (sfxInfor == null || sfxInfor.AudioClip == null || _sfxSourcePrefab == null)
            {
                Debug.LogWarning($"[AudioManager] Khong tim thay SfxInfor cho '{sfx}' hoac prefab/Clip null.");
                return;
            }

            var sfxSource = _poolService.Get(_sfxSourcePrefab);
            var audioSource = sfxSource.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("[AudioManager] SfxSourcePrefab khong co AudioSource component.");
                _poolService.Release(sfxSource);
                return;
            }

            audioSource.clip = sfxInfor.AudioClip;
            audioSource.volume = sfxInfor.Volume;
            audioSource.loop = false;
            audioSource.Play();

            float clipLength = sfxInfor.AudioClip.length;
            Observable.Timer(TimeSpan.FromSeconds(clipLength))
                .Subscribe(_ => _poolService.Release(sfxSource))
                .AddTo(_disposables);
        }

        void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
