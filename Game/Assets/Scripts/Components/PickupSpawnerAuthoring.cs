using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public class PickupSpawnerAuthoring : MonoBehaviour
    {
        public GameObject pickupShield;
        public GameObject pickupBolt;
        public GameObject powerupShield;

        public int pickupSpawnProbability;

        public class PickupSpawnerAuthoringBaker : Baker<PickupSpawnerAuthoring>
        {
            public override void Bake(PickupSpawnerAuthoring authoring)
            {
                AddComponent(new PickupSpawner
                {
                    pickupShield = GetEntity(authoring.pickupShield),
                    pickupBolt = GetEntity(authoring.pickupBolt),
                    powerupShield = GetEntity(authoring.powerupShield),

                    pickupSpawnProbability = authoring.pickupSpawnProbability,
                });


            }
        }

    }

    public struct PickupSpawner : IComponentData
    {
        public Entity pickupShield;
        public Entity pickupBolt;
        public Entity powerupShield;

        public int pickupSpawnProbability;

    }
}