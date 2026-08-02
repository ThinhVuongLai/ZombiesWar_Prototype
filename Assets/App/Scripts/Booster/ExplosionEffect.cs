using App.Core.Services;
using MagicTile.Pool;
using R3;
using UnityEngine;

namespace App.Booster
{
    public class ExplosionEffect : MonoBehaviour, IPoolable
    {
        ParticleSystem _particleSystem;
        PoolService _poolService;
        CompositeDisposable _disposables;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _poolService = ServiceLocator.Resolve<PoolService>();
        }

        public void Play(Vector3 position, float duration)
        {
            transform.position = position;
            gameObject.SetActive(true);

            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            Observable.Timer(System.TimeSpan.FromSeconds(duration))
                .Subscribe(_ =>
                {
                    if (_poolService != null)
                    {
                        _poolService.Release(gameObject);
                    }
                })
                .AddTo(_disposables);
        }

        void IPoolable.OnGetFromPool()
        {
        }

        void IPoolable.OnReleaseToPool()
        {
            _disposables?.Dispose();
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
            }
        }
    }
}
