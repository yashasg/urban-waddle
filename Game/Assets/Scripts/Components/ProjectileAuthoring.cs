using Unity.Entities;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public enum ProjectileType
    {
        Bullet = 0,
        Missle,
    }
    public class  ProjectileAuthoring : MonoBehaviour
    {
        public ProjectileType bulletType;
        public class BulletAuthoringBaker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                AddComponent(new Projectile
                {
                    bulletType = authoring.bulletType
                });
            }
        }
    }
    public struct Projectile : IComponentData
    {
        public ProjectileType bulletType;
    }

}

