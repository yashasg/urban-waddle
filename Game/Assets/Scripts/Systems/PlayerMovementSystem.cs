using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.Collections.LowLevel.Unsafe;

namespace Sandbox.Asteroids
{
    public partial class PlayerMovementSystem : SystemBase
    {

        private EntityQuery cameraQuery;
        private EntityQuery playerQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<Camera>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>());
        }
        protected override void OnUpdate()
        {
            Entity cameraEntity = cameraQuery.ToEntityArray(Allocator.Temp)[0];
            Camera mainCamera = EntityManager.GetComponentObject<Camera>(cameraEntity);
            float cameraDistZ = math.abs(mainCamera.transform.position.z);


            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0,0, cameraDistZ));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, cameraDistZ));
        
            float top = topRight.y;
            float right = topRight.x;

            float bottom = bottomLeft.y;
            float left = bottomLeft.x;

            //hyper drive
            Entities.ForEach((ref LocalTransform localTransform, in PlayerTag playerTag) =>
            {

                float3 pos = localTransform.Position;
                float3 newPos = pos;


                if (pos.x < left)
                {
                    newPos.x = right;
                }
                //right -> left
                if (pos.x > right)
                {
                    newPos.x = left;
                }
                if (pos.y < bottom)
                {
                    newPos.y = top;
                }
                if (pos.y > top)
                {
                    newPos.y = bottom;
                }

                localTransform.Position = newPos;


            }).Schedule();

            float deltaTime = SystemAPI.Time.DeltaTime;
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in PlayerTag playerTag) =>
            {
                //update direction
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                quaternion targetRot = quaternion.LookRotationSafe(math.forward(), normalizedDirection);
                quaternion rotation = math.slerp(transform.Rotation, targetRot, movement.turnSpeed * deltaTime);


                var TRS = float4x4.TRS(newPosition, rotation, math.float3(1.0f));
                transform = LocalTransform.FromMatrix(TRS);


            }).Schedule();

        }
    }
}
