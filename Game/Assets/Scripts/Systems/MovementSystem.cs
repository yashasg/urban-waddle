using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace Sandbox.Asteroids
{ 
    public partial class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            //update player transform
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in PlayerTag playerTag) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                transform.Position += normalizedDirection * movement.speed * deltaTime;

                //update direction
                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                quaternion targetRot = quaternion.LookRotationSafe(math.forward(),normalizedDirection);
                transform.Rotation = math.slerp(transform.Rotation, targetRot, movement.turnSpeed * deltaTime);

            }).Run();

            //update asteroid transforms
            //TODO: schedule parallel
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in AsteroidTag asteroidTag) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                transform.Position += normalizedDirection * movement.speed * deltaTime;

                //TODO:update direction
                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                //quaternion targetRot = quaternion.LookRotationSafe(math.forward(), normalizedDirection);
                //transform.Rotation = math.slerp(transform.Rotation, targetRot, playerMovement.turnSpeed * deltaTime);

            }).Run();

            //update bullet transforms
            //TODO: schedule parallel
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in BulletTag asteroidTag) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                transform.Position += normalizedDirection * movement.speed * deltaTime;

                //TODO:update direction
                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                //quaternion targetRot = quaternion.LookRotationSafe(math.forward(), normalizedDirection);
                //transform.Rotation = math.slerp(transform.Rotation, targetRot, playerMovement.turnSpeed * deltaTime);

            }).Run();

        }
    }
}
