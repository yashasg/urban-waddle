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
        private 

        struct ProjectileOnTriggerSystemJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<Projectile> allProjectiles;
            [ReadOnly]
            public ComponentLookup<Asteroid> allAsteroids;

            public EntityCommandBuffer commandBuffer;
            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                bool isEntityAProjectile = allProjectiles.HasComponent(entityA);
                bool isEntityBProjectile = allProjectiles.HasComponent(entityB);

                if (!isEntityAProjectile && !isEntityBProjectile)
                {
                    //neither of the entities are projectile
                    //we dont handle this here
                    return;
                }

                /* ignore same kind collisions*/
                if (isEntityAProjectile && isEntityBProjectile)
                {
                    return;
                }

                bool isEntityAAsteroid = allAsteroids.HasComponent(entityA);
                bool isEntityBAsteroid = allAsteroids.HasComponent(entityB);
                if (isEntityAAsteroid && isEntityBAsteroid)
                {
                    return;
                }
                Entity projectileEntity = isEntityAProjectile ? entityA : entityB;
                Entity asteroidEntity = isEntityAAsteroid ? entityA : entityB;

                commandBuffer.DestroyEntity(projectileEntity);
                //commandBuffer.DestroyEntity(asteroidEntity);

                UnityEngine.Debug.Log((isEntityAProjectile ? "ProjectileEntityA " : "ProjectileEntityB ") + "collided with " + (isEntityAAsteroid ? "AsteroidA" : "AsteroidB"));
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
            var projectileJob = new ProjectileOnTriggerSystemJob
            {
                allProjectiles = GetComponentLookup<Projectile>(true /*isreadonly*/),
                allAsteroids = GetComponentLookup<Asteroid>(true /*isreadonly*/),
                commandBuffer = commandBuffer

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
            projectileJob.Complete();

        }
    }
}
