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
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class ProjectileTriggerSystem : SystemBase
    {
        [BurstCompile]
        struct OnTriggerSystemJob : ITriggerEventsJob
        {

            [ReadOnly]
            public ComponentLookup<Projectile> allProjectiles;
            [ReadOnly]
            public ComponentLookup<Asteroid> allAsteroids;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Destroyable> allDestroyables;

            bool HandleProjectileTrigger(Entity entityA, Entity entityB)
            {

                bool isEntityAProjectile = allProjectiles.HasComponent(entityA);
                bool isEntityBProjectile = allProjectiles.HasComponent(entityB);

                if (!isEntityAProjectile && !isEntityBProjectile)
                {
                    //neither of the entities are projectile
                    //we dont handle this here
                    return false;
                }

                /* ignore same kind collisions*/
                if (isEntityAProjectile && isEntityBProjectile)
                {
                    return false;
                }

                bool isEntityAAsteroid = allAsteroids.HasComponent(entityA);
                bool isEntityBAsteroid = allAsteroids.HasComponent(entityB);
                if (isEntityAAsteroid && isEntityBAsteroid)
                {
                    return false;
                }

                Destroyable destroyableA = allDestroyables[entityA];
                Destroyable destroyableB = allDestroyables[entityB];

                bool isEntityAMarked = destroyableA.markForDestroy;
                bool isEntityBMarked = destroyableB.markForDestroy;

                if(isEntityAMarked || isEntityBMarked)
                {
                    return false;
                }

                Entity projectileEntity = isEntityAProjectile ? entityA : entityB;
                Entity asteroidEntity = isEntityAAsteroid ? entityA : entityB;


                destroyableA.markForDestroy = true;
                destroyableB.markForDestroy = true;


                allDestroyables[entityA] = destroyableA;
                allDestroyables[entityB] = destroyableB;

                //UnityEngine.Debug.LogWarning(string.Format("ProjectileEntity {0} collided with AsteroidEntity {1}", projectileEntity.Index, asteroidEntity.Index));

                return true;
            }
        

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;
               if(!HandleProjectileTrigger(entityA,entityB))
                {
                    return;
                }
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
            Dependency = new OnTriggerSystemJob
            {

                allProjectiles = GetComponentLookup<Projectile>(true /*isreadonly*/),
                allAsteroids = GetComponentLookup<Asteroid>(true /*isreadonly*/),
                allDestroyables = GetComponentLookup<Destroyable>(),

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
        }
    }
}
