using Dreamy.Core;
using UnityEngine;

namespace Dreamy.Template.Pooling
{
    public static class Pools
    {
        private static IPoolService Service => ServiceLocator.Get<IPoolService>();

        public static GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            return Service.Spawn(prefab, position, rotation, parent);
        }

        public static T Spawn<T>(
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            return Service.Spawn(prefab, position, rotation, parent);
        }

        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Service.Spawn(prefab, parent);
        }

        public static T Spawn<T>(T prefab, Transform parent)
            where T : Component
        {
            return Service.Spawn(prefab, parent);
        }

        // New APIs to spawn by GameObject prefab and automatically GetComponent<T>
        public static T Spawn<T>(GameObject prefab, Transform parent)
            where T : Component
        {
            return Service.Spawn<T>(prefab, parent);
        }

        public static T Spawn<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            return Service.Spawn<T>(prefab, position, rotation, parent);
        }

        public static void Despawn(GameObject instance, float delay = 0f)
        {
            Service.Despawn(instance, delay);
        }

        public static void DespawnAll()
        {
            Service.DespawnAll();
        }
    }
}
