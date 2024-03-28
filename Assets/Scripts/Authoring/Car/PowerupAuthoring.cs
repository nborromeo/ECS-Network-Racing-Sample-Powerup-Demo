using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public struct Powerup : IComponentData { }

    public class PowerupAuthoring : MonoBehaviour
    {
        private class Baker : Baker<PowerupAuthoring>
        {
            public override void Bake(PowerupAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent<Powerup>(entity);
            }
        }
    }
}