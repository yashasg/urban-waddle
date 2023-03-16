using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;


namespace Sandbox.Asteroids
{
    public partial class AsteroidSpawnerSystem : SystemBase
    {
        private EntityQuery asteroidQuery;
        private EntityQuery playerQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            EntityQuery asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidTag>());
            EntityQuery playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<LocalTransform>());

        }
        protected override void OnUpdate()
        {
            int spawnedAsteroids = asteroidQuery.CalculateEntityCount();
            NativeArray<Entity> players = playerQuery.ToEntityArray(Allocator.Temp);


            var localToWorldComponentLookup = GetComponentLookup<LocalToWorld>();
            var transformComponentLookup = GetComponentLookup<LocalTransform>();
            var movementComponentLookup = GetComponentLookup<Movement>();

            var playerPos = localToWorldComponentLookup[players[0]].Position;

            //spawn the large asteroids
            Entities.ForEach((in AsteroidSpawner spawner) =>
            {
                //if current session is not complete
                if (spawnedAsteroids > 0)
                {
                    return;
                }

                //spawn entities
                int entitiesToSpawn = spawner.asteroidCountPerSession;
                NativeArray<Entity> asteroidEntities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(entitiesToSpawn, ref World.Unmanaged.UpdateAllocator);
                EntityManager.Instantiate(spawner.asteroidBig, asteroidEntities);      //update the position of the spawned entities
                foreach (var asteroid in asteroidEntities)
                {
                    //update local to world
                    uint seed = (uint)(asteroid.Index + 1) * 0x9F6ABC1;
                    Random random = new Random(seed);
                    float3 dir = math.float3(math.normalizesafe(random.NextFloat2()), 0);
                    float3 pos = dir * spawner.spawnRadius;

                    LocalToWorld localToWorld = new LocalToWorld
                    {
                        Value = float4x4.TRS(pos, quaternion.identity, math.float3(1.0f))
                    };
                    
                    localToWorldComponentLookup[asteroid] = localToWorld;

                    //update transform
                    LocalTransform transform = transformComponentLookup[asteroid];
                    transform.Position = pos;
                    transformComponentLookup[asteroid] = transform;


                    //update direction
                    Movement movement = movementComponentLookup[asteroid];
                    float3 asteroidDirection = math.normalizesafe(playerPos - pos);
                    movement.direction = math.float2(asteroidDirection.x, asteroidDirection.y);

                    movementComponentLookup[asteroid] = movement;


                }


            }).WithStructuralChanges().Run();

        }
    }
}
