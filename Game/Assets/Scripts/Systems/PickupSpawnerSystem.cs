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
using System;

namespace Sandbox.Asteroids
{
    [UpdateBefore(typeof(DestroyableSystem))]
    public partial class PickupSpawnerSystem : SystemBase
    {
        [BurstCompile]
        struct PickupSpawnerSystemPickupPlacementJob : IJob
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransformLookup;

            [ReadOnly]
            public Entity parent;
            [ReadOnly]
            public Entity child;
            public void Execute()
            {
                LocalTransform parentTransform = localTransformLookup[parent];
                localTransformLookup[child] = parentTransform;
            }
        }

        private EntityQuery pickupSpawnerQuery;
        private EntityQuery asteroidQuery;

        private EntityQuery pickupQuery;
        private EntityQuery playerQuery;

        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();

            pickupQuery = GetEntityQuery(ComponentType.ReadOnly<Pickup>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<PlayerInput>());

            pickupSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<PickupSpawner>());
            asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<Asteroid>());

        }
        [BurstCompile]
        protected override void OnUpdate()
        {

            var pickupSpawners = pickupSpawnerQuery.ToComponentDataArray<PickupSpawner>(Allocator.Temp);

            if(pickupSpawners.Length <= 0)
            {
                return;
            }
            PickupSpawner spawner = pickupSpawners[0];

            //spawn pickups
            var asteroids = asteroidQuery.ToEntityArray(Allocator.Temp);
            var destroyableLookup = GetComponentLookup<Destroyable>(true /*isreadonly*/);

            foreach (var asteroid in asteroids)
            {
                var destroyableAsteroid = destroyableLookup[asteroid];
                if (!destroyableAsteroid.markForDestroy)
                {
                    continue;
                }

                uint seed = (uint)(asteroid.Index + asteroid.Index + 1) * 0x9F6ABC1;
                var random = new Unity.Mathematics.Random(seed);
                if (random.NextInt(0, 100) > spawner.pickupSpawnProbability)
                {
                    continue;
                }

                var randomPickup = random.NextBool() ? spawner.pickupShield : spawner.pickupBolt;
                var pickup = EntityManager.Instantiate(spawner.pickupShield);
                Dependency = new PickupSpawnerSystemPickupPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    parent = asteroid,
                    child = pickup,

                }.Schedule(Dependency);

            }

            var pickups = pickupQuery.ToEntityArray(Allocator.Temp);
            var player = playerQuery.ToEntityArray(Allocator.Temp)[0];
            foreach (var pickup in pickups)
            {
                var destroyablePickup = destroyableLookup[pickup];
                if (!destroyablePickup.markForDestroy)
                {
                    continue;
                }

                var powerup = EntityManager.Instantiate(spawner.powerupShield);
                Dependency = new PickupSpawnerSystemPickupPlacementJob
                {
                    localTransformLookup = GetComponentLookup<LocalTransform>(),
                    parent = player,
                    child = powerup,

                }.Schedule(Dependency);


            }



        }


        private Entity GetEntityToSpawn(AsteroidSpawner spawner, AsteroidSize size)
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
