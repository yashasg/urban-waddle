using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Transforms;

namespace Sandbox.Asteroids
{
    public class PlayerInputAuthoring : MonoBehaviour
    {
        public KeyCode Up;
        public KeyCode Down;
        public KeyCode Left;
        public KeyCode Right;
        public KeyCode Action1;
        public KeyCode Action2;

        public class PlayerInputAuthoringBaker : Baker<PlayerInputAuthoring>
        {
            public override void Bake(PlayerInputAuthoring authoring)
            {
                AddComponent(new PlayerInput
                {
                    Up = authoring.Up,
                    Down = authoring.Down,
                    Left = authoring.Left,
                    Right = authoring.Right,
                    Action1 = authoring.Action1,
                    Action2 = authoring.Action2
                });
            }
        }
    }

    [Serializable]
    public struct PlayerInput : IComponentData
    {
        public KeyCode Up;
        public KeyCode Down;
        public KeyCode Left;
        public KeyCode Right;
        public KeyCode Action1;
        public KeyCode Action2;
    }


}


