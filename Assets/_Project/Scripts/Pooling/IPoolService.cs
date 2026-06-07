using UnityEngine;

namespace Dreamy.Template.Pooling
{
    public interface IPoolService
    {
        GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);

        T Spawn<T>(
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component;

        void Despawn(GameObject instance, float delay = 0f);

        void Preload(
            GameObject prefab,
            int count,
            int capacity = 0,
            bool persistent = false);

        void DespawnAll();
    }
}
