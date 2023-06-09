using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Physics.Systems;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Sandbox.Asteroids
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class PickupTriggerSystem : SystemBase
    {
        [BurstCompile]
        struct PickupOnTriggerSystemJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<Pickup> allPickups;
            [ReadOnly]
            public ComponentLookup<PlayerTag> allPlayers;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Destroyable> allDestroyables;
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

                //UnityEngine.Debug.Log((isEntityAPickup ? "PickupEntityA " : "PickupEntityB ") + "collided with " + (isEntityAPlayer ? "PlayerA" : "PlayerB"));

                Destroyable destroyable = allDestroyables[pickupEntity];
                destroyable.markForDestroy = true;
                allDestroyables[pickupEntity] = destroyable;

            }
        }
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();
        }
        [BurstCompile]
        protected override void OnUpdate()
        {

            Dependency = new PickupOnTriggerSystemJob
            {
                allPickups = GetComponentLookup<Pickup>(true /*isreadonly*/),
                allPlayers = GetComponentLookup<PlayerTag>(true /*isreadonly*/),
                allDestroyables = GetComponentLookup<Destroyable>(),

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        }
    }
}
