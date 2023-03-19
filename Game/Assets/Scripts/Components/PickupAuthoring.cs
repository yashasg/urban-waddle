using Unity.Entities;
using UnityEngine;

namespace Sandbox.Asteroids
{
    public enum PickupType
    {
        Shield = 0,
        Rocket,
        Autoattack
    }
    public class PickupAuthoring : MonoBehaviour
    {
        public PickupType pickupType;
        public class PickupAuthoringBaker : Baker<PickupAuthoring>
        {
            public override void Bake(PickupAuthoring authoring)
            {
                AddComponent(new Pickup
                {
                    pickupType = authoring.pickupType
                });
            }
        }
    }
    public struct Pickup : IComponentData
    {
        public PickupType pickupType;
    }

}

