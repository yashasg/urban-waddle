using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerShipAuthoring : MonoBehaviour
    {
        public int health;
        public float shieldDuration;
        public SpriteRenderer spriteRenderer;

        public class PlayerShipAuthoringBaker : Baker<PlayerShipAuthoring>
        {
            public override void Bake(PlayerShipAuthoring authoring)
            {
                AddComponent(new PlayerShip
                {
                    health = authoring.health,
                    shieldDuration = authoring.shieldDuration,
                    turretOffset = math.float3(0, authoring.spriteRenderer.localBounds.extents.y,0)
                });
            }
        }
    }

    [Serializable]
    public struct PlayerShip : IComponentData
    {

        public int health;
        public float shieldDuration;
        public float3 turretOffset;
    }


}


