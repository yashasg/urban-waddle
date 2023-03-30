using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public class ProjectileSpawnerAuthoring : MonoBehaviour
    {
        public GameObject bullet;
        public GameObject missle;
        public GameObject ufoBullet;
        public int timeBetweenPlayerProjectilesMS = 0;
        public int timeBetweenUfoProjectilesMS = 0;

        public class ProjectileSpawnerAuthoringBaker : Baker<ProjectileSpawnerAuthoring>
        {
            public override void Bake(ProjectileSpawnerAuthoring authoring)
            {
                AddComponent(new ProjectileSpawner
                {
                    bullet = GetEntity(authoring.bullet),
                    missle = GetEntity(authoring.missle),
                    ufoBullet= GetEntity(authoring.ufoBullet),
                    timeBetweenPlayerProjectilesMS = authoring.timeBetweenPlayerProjectilesMS,
                    timeBetweenUfoProjectilesMS = authoring.timeBetweenUfoProjectilesMS
                });


            }
        }

    }

    public struct ProjectileSpawner : IComponentData
    {
        public Entity bullet;
        public Entity missle;
        public Entity ufoBullet;
        public int timeBetweenPlayerProjectilesMS;
        public int timeBetweenUfoProjectilesMS;

    }
}