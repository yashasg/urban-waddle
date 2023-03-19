using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public partial class AsteroidSpawnerSystem : SystemBase
    {
        struct AsteroidSpawnerSystemPlacementJob : IJobFor
        {
            public ComponentLookup<LocalToWorld> localtoWorldLookup;
            public ComponentLookup<LocalTransform> localTransformLookup;
            public ComponentLookup<Movement> movementLookup;

            [ReadOnly]
            public NativeArray<Entity> asteroids;
            [ReadOnly]
            public float3 playerPos;
            [ReadOnly]
            public float spawnerRadius;
            public void Execute(int index)
            {

                Entity asteroid = asteroids[index];
                uint seed = (uint)(asteroid.Index + index + 1) * 0x9F6ABC1;
                var random = new Random(seed);
                var dir = math.float3(math.normalizesafe(random.NextFloat2()),0);
                var pos = float3.zero + (dir * spawnerRadius);
                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(pos, quaternion.LookRotationSafe(math.forward(), dir), math.float3(1.0f))
                };
                localtoWorldLookup[asteroid] = localToWorld;



                //update transform
                LocalTransform transform = localTransformLookup[asteroid];
                transform.Position = pos;
                localTransformLookup[asteroid] = transform;


                //update direction
                Movement movement = movementLookup[asteroid];
                float3 asteroidDirection = math.normalizesafe(float3.zero - pos);
                movement.direction = math.float2(asteroidDirection.x, asteroidDirection.y);

                movementLookup[asteroid] = movement;

            }
        }

        private EntityQuery asteroidSpawnerQuery;
        private EntityQuery asteroidQuery;
        
        protected override void OnCreate()
        {
            base.OnCreate();

            asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidTag>());
            asteroidSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidSpawner>());


        }
        protected override void OnUpdate()
        {
            int spawnedAsteroids = asteroidQuery.CalculateEntityCount();

            var asteroidSpawners = asteroidSpawnerQuery.ToComponentDataArray<AsteroidSpawner>(Allocator.Temp);

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
                    localtoWorldLookup = GetComponentLookup<LocalToWorld>(),
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    asteroids = asteroidEntities,
                    playerPos = float3.zero, //TODO: if we want to target asteroid towards the player
                    spawnerRadius = spawner.spawnRadius

                }.Schedule(entitiesToSpawn, Dependency);


                Dependency.Complete();
            }

        }
    }
}
