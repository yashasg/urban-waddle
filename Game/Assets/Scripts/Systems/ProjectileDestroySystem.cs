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
    public partial class ProjectileDestroySystem : SystemBase
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

            if (cameras.Length < 1)
            {
                //cant find any cameras on screen
                return;
            }

            Camera mainCamera = EntityManager.GetComponentObject<Camera>(cameras[0]);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);


            //projectiles dont hyperdrive so we destroy them
            var projectiles = projectileQuery.ToEntityArray(Allocator.Temp);
            var transformLookup = GetComponentLookup<LocalTransform>();
            for (int i = 0; i < projectiles.Length; ++i)
            {
                var projectile = projectiles[i];
                var localTransform = transformLookup[projectile];
                var viewportPoint = GetViewportPoint(localTransform.Position, mainCamera);
                if (!IsOutOfBounds(viewportPoint))
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
            }.ScheduleParallel(destroyQuery, Dependency);
            Dependency.Complete();

        }

        private Vector3 GetViewportPoint(in float3 worldPos, in Camera mainCamera)
        {
            Vector3 worldPoint = new Vector3(worldPos.x, worldPos.y, mainCamera.nearClipPlane);
            return mainCamera.WorldToViewportPoint(worldPoint);
        }
        private bool IsOutOfBoundsX(in Vector3 viewportPoint)
        {
            return (viewportPoint.x < 0 || viewportPoint.x > 1);
        }
        private bool IsOutOfBoundsY(in Vector3 viewportPoint)
        {

            return (viewportPoint.y < 0 || viewportPoint.y > 1);
        }
        private bool IsOutOfBounds(Vector3 viewportPoint)
        {
            return IsOutOfBoundsX(viewportPoint) || IsOutOfBoundsY(viewportPoint);
        }


        partial struct DestroyJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([ChunkIndexInQuery] int chunkIndex, in Entity entity)
            {
                 ECB.DestroyEntity(chunkIndex,entity);
            }
        }
    }
}
