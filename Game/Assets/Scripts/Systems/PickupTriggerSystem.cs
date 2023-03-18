using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;

namespace Sandbox.Asteroids
{
    public partial class PickupTriggerSystem : SystemBase
    {

        struct PickupOnTriggerSystemJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<Pickup> allPickups;
            [ReadOnly]
            public ComponentLookup<PlayerTag> allPlayers;
            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                bool isEntityAPickup = allPickups.HasComponent(entityA);
                bool isEntityBPickup = allPlayers.HasComponent(entityB);
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
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

        }
        protected override void OnUpdate()
        {
            Dependency = new PickupOnTriggerSystemJob
            {
                allPickups = GetComponentLookup<Pickup>(true /*isreadonly*/),
                allPlayers = GetComponentLookup<PlayerTag>(true /*isreadonly*/)

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
        }
    }
}
