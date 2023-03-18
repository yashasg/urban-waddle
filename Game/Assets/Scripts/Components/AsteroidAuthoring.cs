using Unity.Entities;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public enum AsteroidSize
    {
        Big = 0,
        Med,
        Small,
        Tiny,
        MAX
    }
    public class AsteroidAuthoring : MonoBehaviour
    {
        public AsteroidSize asteroidSize;
        public class AsteroidAuthoringBaker : Baker<AsteroidAuthoring>
        {
            public override void Bake(AsteroidAuthoring authoring)
            {
                AddComponent(new Asteroid
                {
                    asteroidSize = authoring.asteroidSize
                });
            }
        }
    }
    public struct Asteroid : IComponentData
    {
        public AsteroidSize asteroidSize;
    }

}

