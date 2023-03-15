using UnityEngine;
using Unity.Entities;
using System;
using Unity.Mathematics;

namespace Sandbox.Asteroids
{ 
    public partial class PlayerInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref PlayerMovement playerMovement, in PlayerInput playerInput) =>
            {
                Vector2 direction = new Vector2();

                if(Input.GetKey(playerInput.Up))
                {
                    direction += Vector2.up;
                }
                if (Input.GetKey(playerInput.Down))
                {
                    direction += Vector2.down;
                }
                if (Input.GetKey(playerInput.Left))
                {
                    direction += Vector2.left;
                }
                if (Input.GetKey(playerInput.Right))
                {
                    direction += Vector2.right;
                }

                playerMovement.direction.x = direction.x;
                playerMovement.direction.y = direction.y;
                //if (Input.GetKey(playerInput.Action1))
                //{
                //    direction += Vector2.up;
                //}
                //if (Input.GetKey(playerInput.Up))
                //{
                //    direction += Vector2.up;
                //}
            }).Run();
        }
    }
}
