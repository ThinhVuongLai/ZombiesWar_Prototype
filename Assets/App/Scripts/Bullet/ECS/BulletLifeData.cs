using Unity.Entities;
using Unity.Mathematics;

namespace ZombiesWar.Bullet.ECS
{
    public struct BulletLifeData : IComponentData
    {
        public float RemainingLife;
        public float3 StartPosition;
        public float3 PreviousPosition;
        public bool HasHit;
    }
}
