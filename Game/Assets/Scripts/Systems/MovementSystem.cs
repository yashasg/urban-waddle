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
            Dependency = Entities.WithNone<PlayerTag>().ForEach((ref LocalTransform transform, in Movement movement) =>
            {
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

                //update rotation
                quaternion rotation = transform.Rotation;
                var TRS = float4x4.TRS(newPosition, rotation, math.float3(1.0f));

                transform = LocalTransform.FromMatrix(TRS);
            }).ScheduleParallel(Dependency);

        }
    }
}
