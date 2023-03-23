using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public class ProjectileSpawnerAuthoring : MonoBehaviour
    {
        public GameObject bullet;
        public GameObject missle;
        public int timeBetweenProjectilesMS = 0;

        public class ProjectileSpawnerAuthoringBaker : Baker<ProjectileSpawnerAuthoring>
        {
            public override void Bake(ProjectileSpawnerAuthoring authoring)
            {
                AddComponent(new ProjectileSpawner
                {
                    bullet = GetEntity(authoring.bullet),
                    missle = GetEntity(authoring.missle),
                    timeBetweenProjectilesMS = authoring.timeBetweenProjectilesMS
                });


            }
        }

    }

    public struct ProjectileSpawner : IComponentData
    {
        public Entity bullet;
        public Entity missle; 
        public int timeBetweenProjectilesMS;

    }
}