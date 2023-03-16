using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public class AsteroidSpawnerAuthoring : MonoBehaviour
    {
        public GameObject asteroidBig;
        public GameObject asteroidMed;
        public GameObject asteroidSmall;
        public GameObject asteroidTiny;
        public float spawnRadius;
        public int asteroidCountPerSession;

        public class AsteroidSpawnerAuthoringBaker : Baker<AsteroidSpawnerAuthoring>
        {
            public override void Bake(AsteroidSpawnerAuthoring authoring)
            {
                AddComponent(new AsteroidSpawner
                {
                    asteroidBig = GetEntity(authoring.asteroidBig),
                    asteroidMed = GetEntity(authoring.asteroidMed),
                    asteroidSmall = GetEntity(authoring.asteroidSmall),
                    asteroidTiny = GetEntity(authoring.asteroidTiny),

                    asteroidCountPerSession = authoring.asteroidCountPerSession,
                    spawnRadius = authoring.spawnRadius,
                }) ;


            }
        }

    }

    public struct AsteroidSpawner : IComponentData
    {
        public Entity asteroidBig;
        public Entity asteroidMed;
        public Entity asteroidSmall;
        public Entity asteroidTiny;
        public float spawnRadius;
        public int asteroidCountPerSession;

    }
}