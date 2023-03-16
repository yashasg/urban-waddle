using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class AsteroidTagAuthoring : MonoBehaviour
    {
        public class AsteroidTagAuthoringBaker : Baker<AsteroidTagAuthoring>
        {
            public override void Bake(AsteroidTagAuthoring authoring)
            {
                AddComponent(new AsteroidTag
                {
                });
            }
        }
    }

    public struct AsteroidTag : IComponentData
    {
    }


}


