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

        private EntityQuery projectileSpawnerQuery;
        private EntityQuery playerQuery;
        private EntityQuery ufoQuery;
        private int remainingTimeSincePlayerProjectileMS;
        private int remainingTimeSinceUfoProjectileMS;
        private int ufoProjectileSpawnCount = 0;
        protected override void OnCreate()
        {
            base.OnCreate();

            projectileSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<ProjectileSpawner>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerInput>(), ComponentType.ReadOnly<Movement>(), ComponentType.ReadOnly<PlayerShip>());
            ufoQuery = GetEntityQuery(ComponentType.ReadOnly<Ufo>(), ComponentType.ReadOnly<Movement>());


        }

        protected override void OnUpdate()
        {

            var projectileSpawners = projectileSpawnerQuery.ToComponentDataArray<ProjectileSpawner>(Allocator.Temp);


            SpawnPlayerProjectile(projectileSpawners);

            SpawnUfoProjectile(projectileSpawners);

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
