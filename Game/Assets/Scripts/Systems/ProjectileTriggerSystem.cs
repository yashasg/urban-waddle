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
    public partial class ProjectileTriggerSystem : SystemBase
    {
        struct OnTriggerSystemJob : ITriggerEventsJob
        {

            [ReadOnly]
            public ComponentLookup<Projectile> allProjectiles;
            [ReadOnly]
            public ComponentLookup<Asteroid> allAsteroids;

            public EntityCommandBuffer commandBuffer;

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
                Entity projectileEntity = isEntityAProjectile ? entityA : entityB;
                Entity asteroidEntity = isEntityAAsteroid ? entityA : entityB;

                commandBuffer.DestroyEntity(projectileEntity);
                commandBuffer.DestroyEntity(asteroidEntity);

                UnityEngine.Debug.LogFormat("ProjectileEntity {0} collided with AsteroidEntity {1}", projectileEntity.Index, asteroidEntity.Index);

                return true;
            }
        

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;
               if(HandleProjectileTrigger(entityA,entityB))
                {
                    return;
                }
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
            var projectileJob = new OnTriggerSystemJob
            {

                allProjectiles = GetComponentLookup<Projectile>(true /*isreadonly*/),
                allAsteroids = GetComponentLookup<Asteroid>(true /*isreadonly*/),

                commandBuffer = commandBuffer

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
            projectileJob.Complete();

        }
    }
}
