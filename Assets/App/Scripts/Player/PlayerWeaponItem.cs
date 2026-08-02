using System;
using App.Audio;
using App.Core.Services;
using R3;
using UnityEngine;

namespace App.Player
{
    public class PlayerWeaponItem : MonoBehaviour
    {
        [SerializeField] private float _attackEffectBeginTime = 0.5f;
        [SerializeField] private GameObject _attackEffect;
        [SerializeField] private ParticleSystem _attackParticle;
        [SerializeField] private SFX _sfx = SFX.ShotGun;

        private IDisposable _effectTimer;

        public float AttackEffectBeginTime => _attackEffectBeginTime;

        public void ScheduleAttackEffect()
        {
            CancelAttackEffect();

            if (_attackEffect == null) return;

            _effectTimer = Observable.Timer(TimeSpan.FromSeconds(AttackEffectBeginTime))
                .Subscribe(_ => PlayAttackEffect());
        }

        public void Init()
        {
            if (_attackEffect)
            {
                _attackEffect.SetActive(false);
            }
        }

        void PlayAttackEffect()
        {
            if (_attackEffect == null) return;

            if (_attackEffect.activeSelf)
            {
                if (_attackParticle != null)
                {
                    _attackParticle.Play(true);
                }
            }
            else
            {
                _attackEffect.SetActive(true);
            }

            ServiceLocator.Resolve<AudioManager>()?.RunSfx(_sfx);
        }

        public void CancelAttackEffect()
        {
            _effectTimer?.Dispose();
            _effectTimer = null;

            if (_attackEffect == null) return;

            if (_attackParticle != null)
            {
                _attackParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            _attackEffect.SetActive(false);
        }
    }
}
