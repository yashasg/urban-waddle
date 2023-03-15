using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class PlayerMovementAuthoring : MonoBehaviour
    {
        public float2 direction;
        public float speed;
        public float turnSpeed;

        public class PlayerMovementAuthoringBaker : Baker<PlayerMovementAuthoring>
        {
            public override void Bake(PlayerMovementAuthoring authoring)
            {
                AddComponent(new PlayerMovement
                {
                    direction = authoring.direction,
                    speed = authoring.speed,
                    turnSpeed = authoring.turnSpeed,
                });
            }
        }
    }

    [Serializable]
    public struct PlayerMovement : IComponentData
    {
        public float2 direction;
        public float speed;
        public float turnSpeed;
    }


}


