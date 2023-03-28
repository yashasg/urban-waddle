using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class ShieldAuthoring : MonoBehaviour
    {
        public int health;
        public float durationMS;

        public class ShieldAuthoringBaker : Baker<ShieldAuthoring>
        {
            public override void Bake(ShieldAuthoring authoring)
            {
                AddComponent(new Shield
                {
                    health = authoring.health,
                    durationMS = authoring.durationMS,
                });
            }
        }
    }

    [Serializable]
    [WriteGroup(typeof(LocalTransform))]
    public struct Shield : IComponentData
    {
        public float health;
        public float durationMS;
    }


}


