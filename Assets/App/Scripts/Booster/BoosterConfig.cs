using UnityEngine;

namespace App.Booster
{
    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "ZombiesWar/Booster Config")]
    public class BoosterConfig : ScriptableObject
    {
        [Header("Rocket")]
        [SerializeField] GameObject _rocketPrefab;
        [SerializeField] float _explosionRadius = 5f;
        [SerializeField] float _damage = 100f;
        [SerializeField] float _spawnHeight = 15f;
        [SerializeField] float _fallSpeed = 30f;
        [SerializeField] float _cooldown = 10f;

        [Header("Effect")]
        [SerializeField] GameObject _explosionEffectPrefab;
        [SerializeField] float _effectDuration = 2f;

        public GameObject RocketPrefab => _rocketPrefab;
        public float ExplosionRadius => _explosionRadius;
        public float Damage => _damage;
        public float SpawnHeight => _spawnHeight;
        public float FallSpeed => _fallSpeed;
        public float Cooldown => _cooldown;
        public GameObject ExplosionEffectPrefab => _explosionEffectPrefab;
        public float EffectDuration => _effectDuration;
    }
}
