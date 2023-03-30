using Unity.Entities;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public enum ProjectileType
    {
        Bullet = 0,
        Missle,
    }
    public enum ProjectileOwner
    {
        Player =0,
        Enemy
    };
    public class  ProjectileAuthoring : MonoBehaviour
    {
        public ProjectileType bulletType;
        public ProjectileOwner owner;
        public class BulletAuthoringBaker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                AddComponent(new Projectile
                {
                    bulletType = authoring.bulletType,
                    owner = authoring.owner,
                });
            }
        }
    }
    public struct Projectile : IComponentData
    {
        public ProjectileType bulletType;
        public ProjectileOwner owner;
    }

}

