using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class MovementAuthoring : MonoBehaviour
    {
        public float2 direction;
        public float speed;
        public float turnSpeed;

        public class MovementAuthoringBaker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                AddComponent(new Movement
                {
                    direction = authoring.direction,
                    speed = authoring.speed,
                    turnSpeed = authoring.turnSpeed,
                });
            }
        }
    }

    [Serializable]
    public struct Movement : IComponentData
    {
        public float2 direction;
        public float speed;
        public float turnSpeed;
    }


}


