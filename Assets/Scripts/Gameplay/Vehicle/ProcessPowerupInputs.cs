using Unity.Entities.Racing.Authoring;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct ProcessPowerupInputs : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;

            var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (input, powerUp, owner, transform) in SystemAPI.Query<CarInput, PowerupSpawner, GhostOwner, LocalTransform>())
            {
                if (input.Powerup.IsSet)
                {
                    var entity = commandBuffer.Instantiate(powerUp.Prefab);
                    commandBuffer.SetName(entity, "PowerUp");
                    commandBuffer.SetComponent(entity, new GhostOwner {NetworkId = owner.NetworkId});
                    commandBuffer.SetComponent(entity, new LocalTransform
                    {
                        Position = transform.Position,
                        Rotation = quaternion.identity,
                        Scale = 1
                    });
                }
            }
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}