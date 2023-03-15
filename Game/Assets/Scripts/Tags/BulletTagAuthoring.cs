using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class BulletTagAuthoring : MonoBehaviour
    {
        public class BulletTagAuthoringBaker : Baker<BulletTagAuthoring>
        {
            public override void Bake(BulletTagAuthoring authoring)
            {
                AddComponent(new AsteroidTag
                {
                });
            }
        }
    }

    [Serializable]
    public struct BulletTag : IComponentData
    {
    }


}


