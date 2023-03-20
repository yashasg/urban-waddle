using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.Burst;

namespace Sandbox.Asteroids
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class ProjectileDestroySystem : SystemBase
    {

        private EntityQuery projectileQuery;
        private EntityQuery cameraQuery;
        
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();

            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<Camera>());
            projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Projectile>(), ComponentType.ReadOnly<LocalTransform>());

        }
        [BurstCompile]
        protected override void OnUpdate()
        {
            var cameras = cameraQuery.ToEntityArray(Allocator.Temp);

            if (cameras.Length < 1)
            {
                //cant find any cameras on screen
                return;
            }

            Camera mainCamera = EntityManager.GetComponentObject<Camera>(cameras[0]);
            float cameraDistZ = math.abs(mainCamera.transform.position.z);


            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, cameraDistZ));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, cameraDistZ));

            float top = topRight.y;
            float right = topRight.x;

            float bottom = bottomLeft.y;
            float left = bottomLeft.x;

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            //destroy all entities that we tagged with destroytag
            Dependency = new ProjectileDestroySystemDestroyJob
            {
                // Note the function call required to get a parallel writer for an EntityCommandBuffer.
                top = top,
                left = left,
                bottom = bottom,
                right = right,
                ECB = ecb.AsParallelWriter(),
            }.ScheduleParallel(projectileQuery, Dependency);
            Dependency.Complete();

        }
        [BurstCompile]
        partial struct ProjectileDestroySystemDestroyJob : IJobEntity
        {
            public float top;
            public float left;
            public float bottom;
            public float right;

            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([ChunkIndexInQuery] int chunkIndex,in LocalTransform transform, in Entity entity)
            {

                bool bDestroy = false;

                float3 pos = transform.Position;


                bDestroy = pos.x < left;
                bDestroy = pos.x > right;
                bDestroy = pos.y < bottom;
                bDestroy = pos.y > top;

                if(bDestroy)
                {
                    ECB.DestroyEntity(chunkIndex, entity);
                }
                
            }
        }
    }
}
