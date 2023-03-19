using System.Numerics;
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

            //update entity movement and rotation for non players
            Dependency = Entities.WithNone<PlayerTag>().ForEach((ref LocalToWorld transform, in Movement movement) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

                //update rotation
                quaternion rotation = quaternion.RotateZ(math.radians(movement.turnSpeed * deltaTime));


                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(newPosition, transform.Rotation, math.float3(1.0f))
                };
                transform = localToWorld;
            }).ScheduleParallel(Dependency);


            //update rotation of player
            Entities.ForEach((ref LocalToWorld transform, in Movement movement, in PlayerTag playerTag) =>
            {
                //update direction
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

                //HACK - the LookRotationSafe function uses Z axis as forward, which causes a 2d sprites to behave weird in this situation
                //the solution was to swap the arguments
                quaternion targetRot = quaternion.LookRotationSafe(math.forward(), normalizedDirection);

                quaternion rotation = math.slerp(transform.Rotation, targetRot, movement.turnSpeed * deltaTime);

                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(newPosition, rotation, math.float3(1.0f))
                };
                transform = localToWorld;

            }).Run();



        }
    }
}
