using System;

namespace ZombiesWar.Bullet
{
    [Serializable]
    public enum BulletMovementType : byte
    {
        RayCast = 0,
        PhysicsProjectile = 1,
    }
}
