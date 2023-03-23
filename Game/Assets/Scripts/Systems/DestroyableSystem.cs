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
    public partial class DestroyableSystem : SystemBase
    {

        private EntityQuery projectileQuery;
        private EntityQuery asteroidQuery;
        private EntityQuery cameraQuery;
        private EntityQuery destroyableQuery;
        
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();

            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<Camera>());

            projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Projectile>(), ComponentType.ReadOnly<LocalTransform>());
            asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<Asteroid>(), ComponentType.ReadOnly<LocalTransform>());
            destroyableQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(),ComponentType.ReadOnly<Destroyable>());

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

            var destroyProjectiles = new DestroyableSystemEntityVisibilityJob
            {
                top = top,
                left = left,
                bottom = bottom,
                right = right,
            }.ScheduleParallel(projectileQuery,Dependency);


            var destroyAsteroids = new DestroyableSystemEntityVisibilityJob
            {
                top = top + 2,
                left = left - 5,
                bottom = bottom - 2,
                right = right + 5,
            }.ScheduleParallel(asteroidQuery, destroyProjectiles);
            destroyAsteroids.Complete();


            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            Dependency = new DestroyableSystemDestroyJob
            {
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel(destroyableQuery, Dependency);

            Dependency.Complete();

        }
        [BurstCompile]
        partial struct DestroyableSystemEntityVisibilityJob : IJobEntity
        {
            public float top;
            public float left;
            public float bottom;
            public float right;
            void Execute([ChunkIndexInQuery] int chunkIndex, ref Destroyable destroyable,in LocalTransform transform, in Entity entity)
            {
                float3 pos = transform.Position;
                if(!destroyable.markForDestroy)
                {
                    destroyable.markForDestroy = (pos.x < left) || (pos.x > right) || (pos.y < bottom) || (pos.y > top);
                }
               

            }
        }

        [BurstCompile]
        partial struct DestroyableSystemDestroyJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([ChunkIndexInQuery] int chunkIndex,in Destroyable destroyable, in Entity entity)
            {

                if (destroyable.markForDestroy)
                {
                    ECB.DestroyEntity(chunkIndex, entity);
                }

            }
        }

    }
}
