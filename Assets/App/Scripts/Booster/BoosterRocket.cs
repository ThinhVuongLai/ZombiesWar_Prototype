using App.Core.Services;
using MagicTile.Pool;
using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Booster
{
    [RequireComponent(typeof(Rigidbody))]
    public class BoosterRocket : MonoBehaviour, IPoolable
    {
        Rigidbody _rigidbody;
        float _damage;
        float _explosionRadius;
        float _fallSpeed;
        GameObject _effectPrefab;
        float _effectDuration;
        bool _exploded;
        PoolService _poolService;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _poolService = ServiceLocator.Resolve<PoolService>();
        }

        public void Initialize(Vector3 targetPosition, float damage, float explosionRadius,
            float spawnHeight, float fallSpeed, GameObject effectPrefab, float effectDuration)
        {
            _damage = damage;
            _explosionRadius = explosionRadius;
            _fallSpeed = fallSpeed;
            _effectPrefab = effectPrefab;
            _effectDuration = effectDuration;
            _exploded = false;

            transform.position = new Vector3(targetPosition.x, spawnHeight, targetPosition.z);
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (_exploded) return;

            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (transform.position.y <= 0f)
            {
                Explode();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_exploded)
            {
                Explode();
            }
        }

        void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            var colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damage);
                }
            }

            if (_effectPrefab != null && _poolService != null)
            {
                var effect = _poolService.Get(_effectPrefab);
                if (effect.TryGetComponent<ExplosionEffect>(out var explosionEffect))
                {
                    explosionEffect.Play(transform.position, _effectDuration);
                }
            }

            if (_poolService != null)
            {
                _poolService.Release(gameObject);
            }
        }

        void IPoolable.OnGetFromPool()
        {
            _exploded = false;
        }

        void IPoolable.OnReleaseToPool()
        {
        }
    }
}
