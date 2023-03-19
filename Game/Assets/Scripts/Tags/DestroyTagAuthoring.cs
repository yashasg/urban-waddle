using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class DestroyTagAuthoring : MonoBehaviour
    {
        public class DestroyTagAuthoringBaker : Baker<DestroyTagAuthoring>
        {
            public override void Bake(DestroyTagAuthoring authoring)
            {
                AddComponent(new DestroyTag
                {
                });
            }
        }
    }
    public struct DestroyTag : IComponentData
    {
    }


}


