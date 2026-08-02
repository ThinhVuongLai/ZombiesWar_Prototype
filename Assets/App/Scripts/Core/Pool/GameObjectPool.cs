using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MagicTile.Pool
{
    /// <summary>
    /// Generic object pool for a single prefab type.
    /// Retrieves inactive instances or instantiates new ones; releases by deactivating and queueing.
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly int _maximumSize;
        private readonly Queue<GameObject> _inactive = new();

        public int InactiveCount => _inactive.Count;

        private int _index = 0;

        public GameObjectPool(GameObject prefab, Transform parent, int prewarmCount = 0, int maximumSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _maximumSize = maximumSize;
            Prewarm(prewarmCount);
        }

        /// <summary>
        /// Gets an instance from the pool (or creates a new one), activates it, and calls IPoolable.OnGetFromPool.
        /// </summary>
        public GameObject Get()
        {
            GameObject instance = null;

            if (_inactive.Count > 0)
            {
                instance = _inactive.Dequeue();
            }
            else
            {
                instance = Object.Instantiate(_prefab, _parent);

#if UNITY_EDITOR
                instance.name = $"{_prefab.name}_{_index}";
                _index++;
#endif
            }

            if (instance.IsDestroyed())
            {
                instance = Object.Instantiate(_prefab, _parent);
#if UNITY_EDITOR
                instance.name = $"{_prefab.name}_{_index}";
                _index++;
#endif
            }
            else
            {
                instance.SetActive(true);
                if (instance.TryGetComponent<IPoolable>(out var poolable))
                    poolable.OnGetFromPool();
            }

            return instance;
        }

        /// <summary>
        /// Deactivates the instance and returns it to the pool. Destroys if pool is full.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
                return;

            if (instance.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnReleaseToPool();

            instance.SetActive(false);

            if (_inactive.Count < _maximumSize)
                _inactive.Enqueue(instance);
            else
                Object.Destroy(instance);
        }

        /// <summary>
        /// Destroys all inactive instances in the pool.
        /// </summary>
        public void Clear()
        {
            while (_inactive.Count > 0)
                Object.Destroy(_inactive.Dequeue());
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Object.Instantiate(_prefab, _parent);
                instance.SetActive(false);
                _inactive.Enqueue(instance);

#if UNITY_EDITOR
                instance.name = $"{_prefab.name}_{_index}";
                _index++;
#endif
            }
        }
    }
}
