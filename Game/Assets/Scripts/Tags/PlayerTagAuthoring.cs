using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        public class PlayerTagAuthoringBaker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                AddComponent(new PlayerTag
                {
                });
            }
        }
    }

    [Serializable]
    public struct PlayerTag : IComponentData
    {
    }


}


