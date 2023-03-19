using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;

namespace Sandbox.Asteroids
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class HyperDriveSystem : SystemBase
    {

        private EntityQuery projectileQuery;
        private EntityQuery cameraQuery;
        private EntityQuery destroyQuery;
        protected override void OnCreate()
        {
            base.OnCreate();

            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<Camera>());
            projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Projectile>(), ComponentType.ReadOnly<LocalTransform>());
            destroyQuery = GetEntityQuery(ComponentType.ReadOnly<DestroyTag>());


        }
        protected override void OnUpdate()
        {
            var cameras = cameraQuery.ToEntityArray(Allocator.Temp);
         
            if(cameras.Length < 1)
            {
                //can find any cameras on screen
                return;
            }

           Camera mainCamera = EntityManager.GetComponentObject<Camera>(cameras[0]);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);


            //projectiles dont hyperdrive so we destroy them
            var projectiles = projectileQuery.ToEntityArray(Allocator.Temp);
            
            for (int i =0; i < projectiles.Length; ++i) 
            {                
                var projectile = projectiles[i];
                var localTransform = GetComponentLookup<LocalTransform>()[projectile];
                
                if (!IsOutOfBounds(localTransform, mainCamera))
                {
                    continue;
                }
                ecb.AddComponent<DestroyTag>(projectile);

            }


            //destroy all entities that we tagged with destroytag
            Dependency = new DestroyJob
            {
                // Note the function call required to get a parallel writer for an EntityCommandBuffer.
                ECB = ecb.AsParallelWriter(),
            }.ScheduleParallel(Dependency);
            Dependency.Complete();

        }

        private bool IsOutOfBounds(LocalTransform transform,Camera mainCamera)
        {
            float3 projectilePos = transform.Position;
            Vector3 worldPoint = new Vector3(projectilePos.x, projectilePos.y, mainCamera.nearClipPlane);

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPoint);

            if (viewportPoint.x < 0 || viewportPoint.x > 1)
            {
                return true;
            }
            if (viewportPoint.y < 0 || viewportPoint.y > 1)
            {
                return true;
            }

            return false;
        }


        partial struct DestroyJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([ChunkIndexInQuery] int chunkIndex, in Entity entity, in DestroyTag destroyTag)
            {
                 ECB.DestroyEntity(chunkIndex,entity);
            }
        }
    }
}
