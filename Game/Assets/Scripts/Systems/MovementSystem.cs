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
            //update positions
            Entities.ForEach((ref LocalTransform transform, in Movement movement) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                transform.Position += normalizedDirection * movement.speed * deltaTime;
            }).ScheduleParallel();

            //update rotation of asteroids
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in AsteroidTag asteroidTag) =>
            {
                transform.Rotation = quaternion.RotateZ(math.radians(movement.turnSpeed * deltaTime));

            }).ScheduleParallel();

            //update rotation of player
            Entities.ForEach((ref LocalTransform transform, in Movement movement, in PlayerTag playerTag) =>
            {
                //update direction
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);

                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                quaternion targetRot = quaternion.LookRotationSafe(math.forward(), normalizedDirection);
                transform.Rotation = math.slerp(transform.Rotation, targetRot, movement.turnSpeed * deltaTime);

            }).Run();



        }
    }
}
