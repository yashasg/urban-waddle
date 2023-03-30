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
                float3 targetDir = math.up();
                if(math.dot(transform.Forward(), targetDir) >= 0.8)
                {
                    targetDir = math.left();
                }
                quaternion targetRot =  quaternion.LookRotationSafe(transform.Forward(), targetDir);
                quaternion rotation = math.slerp(transform.Rotation, targetRot, movement.turnSpeed * deltaTime);

                var TRS = float4x4.TRS(newPosition, rotation, transform.Scale);

                localTransformLookup[movementEntity] = LocalTransform.FromMatrix(TRS);
            }
        }
    }
}
