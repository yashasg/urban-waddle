using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace Sandbox.Asteroids
{ 
    public partial class MovementSystem : SystemBase
    {
        private EntityQuery movementEntityQuery;
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();

            movementEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<LocalTransform>().WithAll<Movement>().WithNone<PlayerTag>().Build(this);
        }
        [BurstCompile]
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;


            var movementEntities = movementEntityQuery.ToEntityArray(WorldUpdateAllocator);

            //update entity movement and rotation for non players
            //Dependency = Entities.WithNone<PlayerTag>().ForEach((ref LocalTransform transform, in Movement movement) =>
            //{
            //    //update position
            //    float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
            //    float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

            //    //update rotation
            //    quaternion rotation = transform.Rotation;
            //    var TRS = float4x4.TRS(newPosition, rotation, math.float3(1.0f));

            //    transform = LocalTransform.FromMatrix(TRS);
            //}).ScheduleParallel(Dependency);


            Dependency = new MovementSystemSteerJob
            {
                localTransformLookup = GetComponentLookup<LocalTransform>(),
                movementLookup = GetComponentLookup<Movement>(true /*isreadonly*/),
                movementEntitites = movementEntities,
                deltaTime = deltaTime,
            }.Schedule(movementEntities.Length, 50, Dependency);

        }
        [BurstCompile]
        private struct MovementSystemSteerJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransformLookup;

            [ReadOnly]
            public ComponentLookup<Movement> movementLookup;
            [ReadOnly]
            public NativeArray<Entity> movementEntitites;
            public float deltaTime;
            public void Execute(int index)
            {
                var movementEntity = movementEntitites[index];
                var transform = localTransformLookup[movementEntity];
                var movement = movementLookup[movementEntity];
                //update position
                float3 normalizedDirection = math.float3(math.normalizesafe(movement.direction), 0);
                float3 newPosition = transform.Position + (normalizedDirection * movement.speed * deltaTime);

                //update rotation
                quaternion rotation = transform.Rotation;
                var TRS = float4x4.TRS(newPosition, rotation, math.float3(1.0f));

                localTransformLookup[movementEntity] = LocalTransform.FromMatrix(TRS);
            }
        }
    }
}
