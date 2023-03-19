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
            public ComponentLookup<LocalToWorld> localtoWorldLookup;
            public ComponentLookup<Movement> movementLookup;


            [ReadOnly]
            public PlayerShip playerShip;
            [ReadOnly]
            public Entity shipEntity;
            [ReadOnly]
            public Entity spawnedProjectile;

            public void Execute()
            {
                LocalToWorld shipLocalToWorld = localtoWorldLookup[shipEntity];
                float3 playerPos = localtoWorldLookup[shipEntity].Position;
                float3 playerDir = math.normalizesafe( math.float3(movementLookup[shipEntity].direction,0));
                float3 projectileDir = playerDir.Equals(float3.zero) ? math.up() : playerDir;
                float3 turretOffset = playerShip.turretOffset;


                float3 projectilePos = playerPos + turretOffset;
                quaternion rotation = quaternion.LookRotationSafe(math.forward(), projectileDir);
                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(projectilePos, rotation, math.float3(1.0f))
                };
                localtoWorldLookup[spawnedProjectile] = localToWorld;


                //update direction
                Movement movement = movementLookup[spawnedProjectile];
                movement.direction = math.float2(projectileDir.x, projectileDir.y);
                movementLookup[spawnedProjectile] = movement;

            }
        }

        private EntityQuery projectileSpawnerQuery;
        private EntityQuery playerQuery;
        private int remainingTimeSinceProjectileMS;
        protected override void OnCreate()
        {
            base.OnCreate();

            projectileSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<ProjectileSpawner>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerInput>(), ComponentType.ReadOnly<Movement>(), ComponentType.ReadOnly<PlayerShip>());


        }
        protected override void OnUpdate()
        {

            var projectileSpawners = projectileSpawnerQuery.ToComponentDataArray<ProjectileSpawner>(Allocator.Temp);
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
            remainingTimeSinceProjectileMS = math.max(remainingTimeSinceProjectileMS - DeltaTimeMS,0);


            if (remainingTimeSinceProjectileMS > 0)
            {
                //we are not ready to shoot yet
                return;
            }


            for (int i = 0; i < projectileSpawners.Length; ++i)
            {

                ProjectileSpawner spawner = projectileSpawners[i];

                //reset the remaining time
                remainingTimeSinceProjectileMS = spawner.timeBetweenProjectilesMS;

                //spawn entities
                Entity spawnedProjectile = EntityManager.Instantiate(spawner.bullet);

                //update the position of the spawned entities
                Dependency = new ProjectileSpawnerSystemPlacementJob
                {
                    localtoWorldLookup = GetComponentLookup<LocalToWorld>(),
                    movementLookup = GetComponentLookup<Movement>(),
                    playerShip = GetComponentLookup<PlayerShip>()[localPlayer],
                    shipEntity = localPlayer,
                    spawnedProjectile = spawnedProjectile

                }.Schedule();

                Dependency.Complete();
            }


        }
    }
}