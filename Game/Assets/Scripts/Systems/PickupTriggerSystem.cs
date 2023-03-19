using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Physics.Systems;

namespace Sandbox.Asteroids
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class PickupTriggerSystem : SystemBase
    {
        struct PickupOnTriggerSystemJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<Pickup> allPickups;
            [ReadOnly]
            public ComponentLookup<PlayerTag> allPlayers;

            public EntityCommandBuffer commandBuffer;
            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                bool isEntityAPickup = allPickups.HasComponent(entityA);
                bool isEntityBPickup = allPickups.HasComponent(entityB);

                if ((isEntityAPickup == false) && (isEntityBPickup == false))
                {
                    //neither of the entities are pickup
                    //we dont handle this here
                    return;
                }

                /* ignore same kind collisions*/
                if (isEntityAPickup && isEntityBPickup)
                {
                    return;
                }
                bool isEntityAPlayer = allPlayers.HasComponent(entityA);
                bool isEntityBPlayer = allPlayers.HasComponent(entityB);
                if (isEntityAPlayer && isEntityBPlayer)
                {
                    return;
                }
                Entity pickupEntity = isEntityAPickup ? entityA : entityB;
                Entity playerEntity = isEntityAPlayer ? entityA : entityB;

                UnityEngine.Debug.Log((isEntityAPickup ? "PickupEntityA " : "PickupEntityB ") + "collided with " + (isEntityAPlayer ? "PlayerA" : "PlayerB"));

                commandBuffer.DestroyEntity(pickupEntity);


            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var endSimulationEntityCommandBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer commandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer(World.Unmanaged);
            var pickupJob = new PickupOnTriggerSystemJob
            {
                allPickups = GetComponentLookup<Pickup>(true /*isreadonly*/),
                allPlayers = GetComponentLookup<PlayerTag>(true /*isreadonly*/),
                commandBuffer = commandBuffer

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
            pickupJob.Complete();

        }
    }
}
