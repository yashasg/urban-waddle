using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class DestroyableAuthoring : MonoBehaviour
    {
        public bool markForDestroy = false;

        public class DestroyableAuthoringBaker : Baker<DestroyableAuthoring>
        {
            public override void Bake(DestroyableAuthoring authoring)
            {
                AddComponent(new Destroyable
                {
                    markForDestroy = authoring.markForDestroy,
                });
            }
        }
    }

    [Serializable]
    public struct Destroyable : IComponentData
    {
        public bool markForDestroy;
    }


}


