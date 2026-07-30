using Unity.Entities;

namespace ZombiesWar.Bullet.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BulletSystemGroup : ComponentSystemGroup
    {
    }
}
