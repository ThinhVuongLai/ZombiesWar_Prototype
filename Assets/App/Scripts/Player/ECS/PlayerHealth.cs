using Unity.Entities;

namespace App.Player.ECS
{
    public struct PlayerHealth : IComponentData
    {
        public float Value;
        public float MaxValue;
    }
}
