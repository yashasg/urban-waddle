using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Sandbox.Asteroids
{
    public partial class PickupTriggerSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem.Singleton endSimulationEntityCommandBufferSystem;

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

                if((isEntityAPickup == false) && (isEntityBPickup == false))
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

                commandBuffer.DestroyEntity(pickupEntity);

                //UnityEngine.Debug.Log((isEntityAPickup ? "PickupEntityA " : "PickupEntityB ") + "collided with " + (isEntityAPlayer ? "PlayerA" : "PlayerB"));
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEntityCommandBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        protected override void OnUpdate()
        {
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
