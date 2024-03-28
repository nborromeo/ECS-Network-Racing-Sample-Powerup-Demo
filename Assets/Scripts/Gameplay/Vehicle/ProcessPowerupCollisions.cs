using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Authoring;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    public partial struct ProcessPowerUpCollisions : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new PowerupTriggersJob
            {
                Wheels = SystemAPI.GetComponentLookup<Wheel>(),
                PowerUps = SystemAPI.GetComponentLookup<Powerup>(true),
                Children = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                Vehicles = SystemAPI.GetComponentLookup<VehicleChassis>(true),
                Owners = SystemAPI.GetComponentLookup<GhostOwner>(true),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }
    }

    [BurstCompile]
    struct PowerupTriggersJob : ITriggerEventsJob
    {
        public ComponentLookup<Wheel> Wheels;
        [ReadOnly] public BufferLookup<LinkedEntityGroup> Children;
        [ReadOnly] public ComponentLookup<VehicleChassis> Vehicles;
        [ReadOnly] public ComponentLookup<GhostOwner> Owners;
        [ReadOnly] public ComponentLookup<Powerup> PowerUps;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            var aIsVehicle = Vehicles.HasComponent(entityA);
            var bIsVehicle = Vehicles.HasComponent(entityB);
            var aIsPowerUp = PowerUps.HasComponent(entityA);
            var bIsPowerUp = PowerUps.HasComponent(entityB);

            var powerUpVehicleTrigger = (aIsVehicle || bIsVehicle) && (aIsPowerUp || bIsPowerUp);

            if (!powerUpVehicleTrigger)
            {
                return;
            }

            var aOwner = Owners[entityA];
            var bOwner = Owners[entityB];
            var isSameOwner = aOwner.NetworkId == bOwner.NetworkId;
            if (isSameOwner)
            {
                return;
            }

            var vehicleEntity = aIsVehicle ? entityA : entityB;
            var vehicleChildren = Children[vehicleEntity];

            for (var i = 0; i < vehicleChildren.Length; i++)
            {
                var wheelEntity = vehicleChildren[i].Value;
                if (Wheels.TryGetComponent(wheelEntity, out var wheel))
                {
                    wheel.GripFactor = 0;
                    Wheels[wheelEntity] = wheel;
                }
            }
        }
    }
}
