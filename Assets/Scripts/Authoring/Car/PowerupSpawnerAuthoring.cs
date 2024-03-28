using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public struct PowerupSpawner : IComponentData
    {
        public Entity Prefab;
    }
    
    public class PowerupSpawnerAuthoring : MonoBehaviour
    {
        public GameObject PowerupPrefab;
        
        private class Baker : Baker<PowerupSpawnerAuthoring>
        {
            public override void Bake(PowerupSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                var powerUpPrefabEntity = GetEntity(authoring.PowerupPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PowerupSpawner
                {
                    Prefab = powerUpPrefabEntity
                });
            }
        }
    }
}