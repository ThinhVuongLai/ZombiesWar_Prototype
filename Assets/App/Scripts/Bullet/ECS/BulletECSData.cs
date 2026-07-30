using Unity.Entities;
using Unity.Mathematics;

namespace ZombiesWar.Bullet.ECS
{
    public struct BulletData : IComponentData
    {
        public float Speed;
        public float Damage;
        public float Lifetime;
        public BulletMovementType MovementType;
        public Entity TargetEntity;
    }
}
