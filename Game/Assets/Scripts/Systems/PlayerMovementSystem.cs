using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace Sandbox.Asteroids
{ 
    public partial class PlayerMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            Entities.ForEach((ref LocalTransform transform, in PlayerMovement playerMovement) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(playerMovement.direction), 0);
                transform.Position += normalizedDirection * playerMovement.speed * deltaTime;

                //update direction
                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                quaternion targetRot = quaternion.LookRotationSafe(math.forward(),normalizedDirection);
                transform.Rotation = math.slerp(transform.Rotation, targetRot, playerMovement.turnSpeed * deltaTime);

            }).Run();
    }
    }
}
