using Unity.Entities;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public class UfoAuthoring : MonoBehaviour
    {
        public int health;
        public int points;
        public int attackDamage;
        public class UfoAuthoringBaker : Baker<UfoAuthoring>
        {
            public override void Bake(UfoAuthoring authoring)
            {
                AddComponent(new Ufo
                {
                    health = authoring.health,
                    points = authoring.points,
                    attackDamage = authoring.attackDamage
                });
            }
        }
    }
    public struct Ufo : IComponentData
    {
        public int health;
        public int points;
        public int attackDamage;
    }

}

