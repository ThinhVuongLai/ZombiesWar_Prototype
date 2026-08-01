using System.Collections.Generic;
using App.Core.Services;
using Unity.VisualScripting;
using UnityEngine;

namespace MagicTile.Pool
{
    /// <summary>
    /// Manages multiple GameObjectPools by prefab. Reusable for tiles, VFX, etc.
    /// Assign prefabs in the inspector to pre-register pools at startup.
    /// </summary>
    public class PoolService : MonoBehaviour
    {
        [System.Serializable]
        public class PoolConfig
        {
            public GameObject prefab;
            [Tooltip("Number of instances to pre-instantiate at startup.")]
            public int prewarmCount = 10;
            [Tooltip("Maximum inactive instances kept in the pool. Excess are destroyed.")]
            public int maximumSize = 100;
        }

        [SerializeField] private PoolConfig[] _configs;

        private readonly Dictionary<GameObject, GameObjectPool> _poolsByPrefab = new();
        private readonly Dictionary<GameObject, GameObjectPool> _poolByInstance = new();

        private void Awake()
        {
            ServiceLocator.Register(this);

            foreach (PoolConfig config in _configs)
            {
                if (config.prefab == null)
                    continue;

                Prewarm(config.prefab, transform, config.prewarmCount, config.maximumSize);
            }
        }

        /// <summary>
        /// Gets an instance of the given prefab from its pool (creates the pool on first request).
        /// </summary>
        public GameObject Get(GameObject prefab)
        {
            GameObjectPool pool = GetOrCreatePool(prefab);
            GameObject instance = pool.Get();
            _poolByInstance[instance] = pool;
            return instance;
        }

        /// <summary>
        /// Generic overload — gets a Component of type T from the prefab's pool.
        /// </summary>
        public T Get<T>(T prefab) where T : Component
        {
            return Get(prefab.gameObject).GetComponent<T>();
        }

        /// <summary>
        /// Returns an instance to its originating pool. Destroys if not from any pool.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
                return;

            if (_poolByInstance.TryGetValue(instance, out GameObjectPool pool))
            {
                _poolByInstance.Remove(instance);
                pool.Release(instance);
            }
            else
            {
                Debug.LogWarning($"[PoolService] Object '{instance.name}' was not from any pool. Destroying.");
                Destroy(instance);
            }
        }

        /// <summary>
        /// Generic overload — releases a Component's GameObject to its pool.
        /// </summary>
        public void Release(Component component)
        {
            if (!component.IsDestroyed())
            {
                Release(component.gameObject);
            }
        }

        /// <summary>
        /// Destroys all inactive instances across all pools and clears registrations.
        /// </summary>
        public void ClearAll()
        {
            foreach (GameObjectPool pool in _poolsByPrefab.Values)
                pool.Clear();

            _poolsByPrefab.Clear();
            _poolByInstance.Clear();
        }

        /// <summary>
        /// Pre-registers a pool and prewarms instances for the given prefab.
        /// Does nothing if a pool for this prefab already exists.
        /// </summary>
        public void Prewarm(GameObject prefab, Transform parent, int prewarmCount, int maximumSize = 100)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[PoolService] Prewarm called with null prefab.");
                return;
            }

            if (_poolsByPrefab.ContainsKey(prefab))
            {
                Debug.LogWarning($"[PoolService] Pool for '{prefab.name}' already exists. Skipping Prewarm.");
                return;
            }

            var pool = new GameObjectPool(prefab, parent, prewarmCount, maximumSize);
            _poolsByPrefab[prefab] = pool;
        }

        private GameObjectPool GetOrCreatePool(GameObject prefab)
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out GameObjectPool pool))
            {
                pool = new GameObjectPool(prefab, transform);
                _poolsByPrefab[prefab] = pool;
            }
            return pool;
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
