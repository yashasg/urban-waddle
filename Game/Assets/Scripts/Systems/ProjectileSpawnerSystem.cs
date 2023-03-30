using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using System;

namespace Sandbox.Asteroids
{
    public partial class ProjectileSpawnerSystem : SystemBase
    {
        struct ProjectileSpawnerSystemPlacementJob : IJob
        {
            public ComponentLookup<LocalTransform> localTransformLookup;
            public ComponentLookup<Movement> movementLookup;


            [ReadOnly]
            public float3 turretOffset;
            [ReadOnly]
            public float3 shipPosition;
            [ReadOnly]
            public quaternion shipRotation;
            [ReadOnly]
            public Entity spawnedProjectile;

            public void Execute()
            {
                float3 projectileDir = math.mul(shipRotation,math.up()); //get the direction based on the player rotation 
                float3 projectilePos = shipPosition + (projectileDir * turretOffset.y);
                var TRS = float4x4.TRS(projectilePos, shipRotation, math.float3(1.0f));

                localTransformLookup[spawnedProjectile] = LocalTransform.FromMatrix(TRS);

                //update direction
                Movement movement = movementLookup[spawnedProjectile];
                movement.direction = math.float2(projectileDir.x, projectileDir.y);
                movementLookup[spawnedProjectile] = movement;

            }
        }


        struct ProjectileSpawnerSystemPursueJob : IJob
        {
            public ComponentLookup<LocalTransform> localTransformLookup;
            public ComponentLookup<Movement> movementLookup;


            [ReadOnly]
            public Entity asteroid;
            [ReadOnly]
            public float3 shipPosition;
            [ReadOnly]
            public Entity spawnedProjectile;

            public void Execute()
            {
                float3 asteroidPos = localTransformLookup[asteroid].Position;
                var asteroidMovement = movementLookup[asteroid];
                float3 asteroidDir = math.float3(asteroidMovement.direction,0);
                float asteroidSpeed = asteroidMovement.speed;


                Movement projectileMovement = movementLookup[spawnedProjectile];

                float3 projectilePos = shipPosition ;
                float3 direction = asteroidPos - shipPosition;
                float T = math.length(direction) / projectileMovement.speed;
                float3 futurePos = asteroidPos + (asteroidDir * asteroidSpeed * T);


                var localTransform = localTransformLookup[spawnedProjectile];
                var TRS = float4x4.TRS(projectilePos, quaternion.LookRotationSafe(math.forward(),direction), localTransform.Scale);

                localTransformLookup[spawnedProjectile] = LocalTransform.FromMatrix(TRS);

                //update direction
                float3 projectileDir = futurePos - shipPosition;
                projectileMovement.direction = math.float2(projectileDir.x, projectileDir.y);
                movementLookup[spawnedProjectile] = projectileMovement;

            }
        }

        private EntityQuery projectileSpawnerQuery;
        private EntityQuery playerQuery;
        private EntityQuery ufoQuery;
        private EntityQuery pickupQuery;
        private EntityQuery asteroidQuery;
        private int remainingTimeSincePlayerProjectileMS;
        private int remainingTimeSinceUfoProjectileMS;
        private int ufoProjectileSpawnCount = 0;
        protected override void OnCreate()
        {
            base.OnCreate();

            projectileSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<ProjectileSpawner>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerInput>(), ComponentType.ReadOnly<Movement>(), ComponentType.ReadOnly<PlayerShip>());
            ufoQuery = GetEntityQuery(ComponentType.ReadOnly<Ufo>(), ComponentType.ReadOnly<Movement>());
            pickupQuery = GetEntityQuery(ComponentType.ReadOnly<Pickup>());
            asteroidQuery = GetEntityQuery(ComponentType.ReadOnly<Asteroid>());

        }

        protected override void OnUpdate()
        {

            var projectileSpawners = projectileSpawnerQuery.ToComponentDataArray<ProjectileSpawner>(Allocator.Temp);
            if(projectileSpawners.Length <= 0)
            {
                return;
            }

            SpawnPlayerProjectile(projectileSpawners);

            SpawnUfoProjectile(projectileSpawners);

            SpawnMissleProjectile(projectileSpawners);

        }

        private void SpawnMissleProjectile( in NativeArray<ProjectileSpawner> projectileSpawners)
        {
            var pickups = pickupQuery.ToEntityArray(Allocator.Temp);
            var players = playerQuery.ToEntityArray(Allocator.Temp);
            if(pickups.Length <= 0)
            {
                return;
            }
            if(players.Length <= 0)
            {
                return;
            }
            var asteroids = asteroidQuery.ToEntityArray(Allocator.Temp);
            var transformLookup = GetComponentLookup<LocalTransform>();
            var pickupLookup = GetComponentLookup<Pickup>(true);
            var destroyableLookup= GetComponentLookup<Destroyable>(true);
            for(int i = 0; i < pickups.Length; ++i) 
            {
                var pickup = pickups[i];

                if (pickupLookup[pickup].pickupType != PickupType.Rocket)
                {
                    continue;
                }
                if (!destroyableLookup[pickup].markForDestroy)
                {
                    continue;
                }

                for(int j = 0; j < asteroids.Length; ++j)
                {
                    var asteroid = asteroids[j];

                    if (destroyableLookup[asteroid].markForDestroy)
                    {
                        continue;
                    }

                    for(int k = 0; k < projectileSpawners.Length; ++k)
                    {
                        var spawner = projectileSpawners[k];

                        Entity spawnedProjectile = EntityManager.Instantiate(spawner.missle);

                        Dependency = new ProjectileSpawnerSystemPursueJob
                        {
                            localTransformLookup = transformLookup,
                            movementLookup = GetComponentLookup<Movement>(),
                            spawnedProjectile = spawnedProjectile,
                            asteroid = asteroid,
                            shipPosition = transformLookup[players[0]].Position

                        }.Schedule(Dependency);


                    }
                    
                }




            }

        }

        private void SpawnPlayerProjectile(in NativeArray<ProjectileSpawner> projectileSpawners)
        {

            var players = playerQuery.ToEntityArray(Allocator.Temp);

            if (players.Length < 1)
            {
                //need atleast 1 player
                return;
            }
            Entity localPlayer = players[0];
            PlayerInput localPlayerInput = GetComponentLookup<PlayerInput>(true)[localPlayer];


            if (!Input.GetKey(localPlayerInput.Action1))
            {
                //player is not attacking so we dont spawn projectiles
                return;
            }

            int DeltaTimeMS = Convert.ToInt32(SystemAPI.Time.DeltaTime * 1000);
            remainingTimeSincePlayerProjectileMS = math.max(remainingTimeSincePlayerProjectileMS - DeltaTimeMS, 0);


            if (remainingTimeSincePlayerProjectileMS > 0)
            {
                //we are not ready to shoot yet
                return;
            }


            for (int i = 0; i < projectileSpawners.Length; ++i)
            {

                ProjectileSpawner spawner = projectileSpawners[i];

                //reset the remaining time
                remainingTimeSincePlayerProjectileMS = spawner.timeBetweenPlayerProjectilesMS;

                //spawn entities
                Entity spawnedProjectile = EntityManager.Instantiate(spawner.bullet);

                //update the position of the spawned entities
                ComponentLookup<LocalTransform> transformLookup = GetComponentLookup<LocalTransform>();
                var localPlayerTransform = transformLookup[localPlayer];
                Dependency = new ProjectileSpawnerSystemPlacementJob
                {
                    localTransformLookup = transformLookup,
                    movementLookup = GetComponentLookup<Movement>(),
                    turretOffset = GetComponentLookup<PlayerShip>()[localPlayer].turretOffset,
                    shipPosition = localPlayerTransform.Position,
                    shipRotation = localPlayerTransform.Rotation,
                    spawnedProjectile = spawnedProjectile

                }.Schedule(Dependency);
            }


        }

        private void SpawnUfoProjectile(in NativeArray<ProjectileSpawner> projectileSpawners)
        {

            var ufos = ufoQuery.ToEntityArray(Allocator.Temp);

            if (ufos.Length < 1)
            {
                //need atleast 1
                return;
            }

            int DeltaTimeMS = Convert.ToInt32(SystemAPI.Time.DeltaTime * 1000);
            remainingTimeSinceUfoProjectileMS = math.max(remainingTimeSinceUfoProjectileMS - DeltaTimeMS, 0);

            if (remainingTimeSinceUfoProjectileMS > 0)
            {
                //we are not ready to shoot yet
                return;
            }
            ufoProjectileSpawnCount = (ufoProjectileSpawnCount + 1) % 4;
            for (int i = 0; i < ufos.Length; ++i)
            {
                var ufo = ufos[i];
                
                for (int j = 0; j < projectileSpawners.Length; ++j)
                {

                    ProjectileSpawner spawner = projectileSpawners[j];

                    //reset the remaining time
                    remainingTimeSinceUfoProjectileMS = spawner.timeBetweenUfoProjectilesMS;

                    //spawn entities
                    Entity spawnedProjectile = EntityManager.Instantiate(spawner.ufoBullet);

                    ComponentLookup<LocalTransform> transformLookup = GetComponentLookup<LocalTransform>();
                    var ufoTransform = transformLookup[ufo];
                    var spawnDir = GetSpawnDirection(ufoProjectileSpawnCount);
                    //update the position of the spawned entities
                    Dependency = new ProjectileSpawnerSystemPlacementJob
                    {
                        localTransformLookup = GetComponentLookup<LocalTransform>(),
                        movementLookup = GetComponentLookup<Movement>(),
                        turretOffset = math.up(),
                        shipPosition = ufoTransform.Position,
                        shipRotation = quaternion.LookRotationSafe(math.forward(), spawnDir),
                        spawnedProjectile = spawnedProjectile

                    }.Schedule(Dependency);
                }

            }

        }

        private float3 GetSpawnDirection(int index)
        {
            if(index == 0)
            {
                return math.up();
            }
            if (index == 1)
            {
                return math.right();
            }
            if (index == 2)
            {
                return math.left();
            }
            if (index == 3)
            {
                return math.down();
            }
            return math.up();
        }
    }
}
