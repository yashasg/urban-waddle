using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.Physics.Systems;

namespace Sandbox.Asteroids
{
    [UpdateBefore(typeof(DestroyableSystem))]
    public partial class AsteroidSpawnerSystem : SystemBase
    {
        [BurstCompile]
        struct AsteroidSpawnerSystemRandomPlacementJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransformLookup;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
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
                var dest = math.float3(random.NextFloat(spawnerRect.c0.x, spawnerRect.c0.y), random.NextFloat(spawnerRect.c1.x, spawnerRect.c1.y), 0);
                var dir = math.normalizesafe(dest - pos);

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
        struct AsteroidSpawnerSystemOrphanPlacementJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransformLookup;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Movement> movementLookup;

            [ReadOnly]
            public Entity parent;
            [ReadOnly]
            public NativeArray<Entity> children;
            public void Execute(int index)
            {

                Entity child = children[index];

                LocalTransform parentTransform = localTransformLookup[parent];
                localTransformLookup[child] = parentTransform;

                float2 parentDirection = movementLookup[parent].direction;
                Movement childMovement = movementLookup[child];
                float2 normal = (index == 0) ? math.float2(-parentDirection.y, parentDirection.x) : math.float2(parentDirection.y, -parentDirection.x);
                childMovement.direction = normal;
                movementLookup[child] = childMovement;

            }
        }

        struct AsteroidSpawnerSystemUFOPlacementJob : IJob
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransformLookup;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Movement> movementLookup;

            [ReadOnly]
            public Entity ufo;
            [ReadOnly]
            public float2x2 spawnerRect;
            public void Execute()
            {
                uint seed = (uint) 0x9F6ABC1;
                var random = new Unity.Mathematics.Random(seed);

                bool2 topLeft = random.NextBool2();
                float posX = topLeft.x ? spawnerRect.c0.x : spawnerRect.c0.y;
                float posY = topLeft.y ? spawnerRect.c1.y : spawnerRect.c1.x;
                var pos = math.float3(posX, posY, 0);
                var dest = math.float3(random.NextFloat(spawnerRect.c0.x, spawnerRect.c0.y), random.NextFloat(spawnerRect.c1.x, spawnerRect.c1.y), 0);
                var dir = math.normalizesafe(dest - pos);

                var TRS = float4x4.TRS(pos, quaternion.identity, math.float3(1.0f));


                //update transform
                localTransformLookup[ufo] = LocalTransform.FromMatrix(TRS);


                //update direction
                Movement movement = movementLookup[ufo];
                movement.direction = math.float2(dir.x, dir.y);
                movementLookup[ufo] = movement;

            }
        }


        private EntityQuery asteroidSpawnerQuery;
        private EntityQuery asteroidQuery;
        private EntityQuery cameraQuery;
        private int currentAsteroidSession = 0;
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

            //spawn the large asteroids
            for (int i = 0; i < asteroidSpawners.Length; ++i)
            {
                AsteroidSpawner spawner = asteroidSpawners[i];
                //if current session is not complete
                if (spawnedAsteroids > 0)
                {
                    break;
                }
                bool spawnUFO = ++currentAsteroidSession >= spawner.ufoSpawnSessionCount;
                //spawn entities
                int entitiesToSpawn = spawner.asteroidCountPerSession;
                NativeArray<Entity> asteroidEntities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(entitiesToSpawn, ref World.Unmanaged.UpdateAllocator);
                EntityManager.Instantiate(spawner.asteroidBig, asteroidEntities);


                float top = topRight.y;
                float right = topRight.x;

                float bottom = bottomLeft.y;
                float left = bottomLeft.x;

                //update the position of the spawned entities
                Dependency = new AsteroidSpawnerSystemRandomPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    asteroids = asteroidEntities,
                    spawnerRect = spawner.spawnRect + math.float2x2(left, bottom, right, top)

                }.Schedule(entitiesToSpawn, entitiesToSpawn, Dependency);


                if(!spawnUFO)
                {
                    continue;
                }
                //spawn UFO
                Entity ufo = EntityManager.Instantiate(spawner.ufo);
                Dependency = new AsteroidSpawnerSystemUFOPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    ufo = ufo,
                    spawnerRect = spawner.spawnRect + math.float2x2(left, bottom, right, top)

                }.Schedule(Dependency);

            }

            //spawn orphan asteroids
            var asteroids = asteroidQuery.ToEntityArray(Allocator.Temp);
            var destroyableLookup = GetComponentLookup<Destroyable>(true /*isreadonly*/);
            var asteroidLookup = GetComponentLookup<Asteroid>(true /*isreadonly*/);

            foreach (var asteroid in asteroids)
            {
                var destroyableAsteroid = destroyableLookup[asteroid];

                if(!destroyableAsteroid.markForDestroy)
                {
                    continue;
                }

                //we were marked for destroy spawn childrent
                int asteroidsToSpawn = 2;
                NativeArray<Entity> asteroidEntities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(asteroidsToSpawn, ref World.Unmanaged.UpdateAllocator);
                Entity prefab = GetEntityToSpawn(asteroidSpawners[0], asteroidLookup[asteroid].asteroidSize);

                if(prefab == Entity.Null)
                {
                    continue;
                }

                EntityManager.Instantiate(prefab, asteroidEntities);
                Dependency = new AsteroidSpawnerSystemOrphanPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    parent = asteroid,
                    children = asteroidEntities,

                }.Schedule(asteroidsToSpawn, asteroidsToSpawn, Dependency);


            }


        }


        private Entity GetEntityToSpawn(AsteroidSpawner spawner,AsteroidSize size)
        {
            //spawn 1 size smaller
            if (size == AsteroidSize.Big)
            {
                return spawner.asteroidMed;
            }

            if (size == AsteroidSize.Med)
            {
                return spawner.asteroidSmall;
            }

            if (size == AsteroidSize.Small)
            {
                return spawner.asteroidTiny;
            }

            return Entity.Null;

        }
    }
}
