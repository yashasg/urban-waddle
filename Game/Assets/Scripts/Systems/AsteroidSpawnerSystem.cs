using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public partial class AsteroidSpawnerSystem : SystemBase
    {
        [BurstCompile]
        struct AsteroidSpawnerSystemPlacementJob : IJobFor
        {
            public ComponentLookup<LocalTransform> localTransformLookup;
            public ComponentLookup<Movement> movementLookup;

            [ReadOnly]
            public NativeArray<Entity> asteroids;
            [ReadOnly]
            public float2x2 spawnerRect;
            public void Execute(int index)
            {

                Entity asteroid = asteroids[index];
                uint seed = (uint)(asteroid.Index + index + 1) * 0x9F6ABC1;
                var random = new Unity.Mathematics.Random(seed);

                bool2 topLeft = random.NextBool2();
                float posX = topLeft.x ? spawnerRect.c0.x : spawnerRect.c0.y;
                float posY = topLeft.y ? spawnerRect.c1.y : spawnerRect.c1.x;
                var pos = math.float3(posX,posY,0);

                var dir = math.float3(math.normalizesafe(random.NextFloat2()), 0);

                var TRS = float4x4.TRS(pos, quaternion.identity, math.float3(1.0f));


                //update transform
                localTransformLookup[asteroid] = LocalTransform.FromMatrix(TRS);


                //update direction
                Movement movement = movementLookup[asteroid];
                float3 asteroidDirection = dir;
                movement.direction = math.float2(asteroidDirection.x, asteroidDirection.y);

                movementLookup[asteroid] = movement;

            }
        }

        private EntityQuery asteroidSpawnerQuery;
        private EntityQuery asteroidQuery;
        private EntityQuery cameraQuery;
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();

            asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidTag>());
            asteroidSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidSpawner>());
            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<Camera>());


        }
        [BurstCompile]
        protected override void OnUpdate()
        {
            int spawnedAsteroids = asteroidQuery.CalculateEntityCount();

            var asteroidSpawners = asteroidSpawnerQuery.ToComponentDataArray<AsteroidSpawner>(Allocator.Temp);

            Entity cameraEntity = cameraQuery.ToEntityArray(Allocator.Temp)[0];
            Camera mainCamera = EntityManager.GetComponentObject<Camera>(cameraEntity);
            float cameraDistZ = math.abs(mainCamera.transform.position.z);


            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, cameraDistZ));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, cameraDistZ));

            float top = topRight.y;
            float right = topRight.x;

            float bottom = bottomLeft.y;
            float left = bottomLeft.x;


            //spawn the large asteroids
            for (int i = 0; i < asteroidSpawners.Length; ++i)
            {
                AsteroidSpawner spawner = asteroidSpawners[i];
                //if current session is not complete
                if (spawnedAsteroids > 0)
                {
                    return;
                }

                //spawn entities
                int entitiesToSpawn = spawner.asteroidCountPerSession;
                NativeArray<Entity> asteroidEntities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(entitiesToSpawn, ref World.Unmanaged.UpdateAllocator);
                EntityManager.Instantiate(spawner.asteroidBig, asteroidEntities);

                //update the position of the spawned entities
                Dependency = new AsteroidSpawnerSystemPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    asteroids = asteroidEntities,
                    spawnerRect = spawner.spawnRect + math.float2x2(left, bottom, right, top)

                }.Schedule(entitiesToSpawn, Dependency);


                Dependency.Complete();
            }

        }
    }
}
