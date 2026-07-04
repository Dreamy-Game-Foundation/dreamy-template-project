using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

namespace Dreamy.Template.Pooling
{
    public sealed class LeanPoolService : IPoolService, IDisposable
    {
        private const string PoolRootName = "[PoolService]";

        private readonly Dictionary<GameObject, LeanGameObjectPool> pools =
            new();

        private readonly Transform poolRoot;

        public LeanPoolService()
        {
            GameObject root = new(PoolRootName);
            UnityEngine.Object.DontDestroyOnLoad(root);
            poolRoot = root.transform;
        }

        public GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return LeanPool.Spawn(prefab, position, rotation, parent);
        }

        public T Spawn<T>(
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return LeanPool.Spawn(prefab, position, rotation, parent);
        }

        public GameObject Spawn(GameObject prefab, Transform parent)
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return LeanPool.Spawn(prefab, parent);
        }

        public T Spawn<T>(T prefab, Transform parent)
            where T : Component
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return LeanPool.Spawn(prefab, parent);
        }

        public T Spawn<T>(GameObject prefab, Transform parent)
            where T : Component
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            GameObject instance = LeanPool.Spawn(prefab, parent);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public T Spawn<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            GameObject instance = LeanPool.Spawn(prefab, position, rotation, parent);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Despawn(GameObject instance, float delay = 0f)
        {
            if (instance)
            {
                LeanPool.Despawn(instance, delay);
            }
        }

        public void Preload(
            GameObject prefab,
            int count,
            int capacity = 0,
            bool persistent = false)
        {
            if (!prefab)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (!pools.TryGetValue(prefab, out LeanGameObjectPool pool) ||
                !pool)
            {
                GameObject poolObject =
                    new($"Pool - {prefab.name}");
                poolObject.transform.SetParent(poolRoot, false);
                pool = poolObject.AddComponent<LeanGameObjectPool>();
                pool.Prefab = prefab;
                pool.Notification =
                    LeanGameObjectPool.NotificationType.IPoolable;
                pool.Persist = persistent;
                pools[prefab] = pool;
            }

            pool.Preload = count;
            pool.Capacity = capacity;
            pool.PreloadAll();
        }

        public void DespawnAll()
        {
            LeanPool.DespawnAll();
        }

        public void Dispose()
        {
            DespawnAll();
            if (poolRoot)
            {
                UnityEngine.Object.Destroy(poolRoot.gameObject);
            }

            pools.Clear();
        }
    }
}