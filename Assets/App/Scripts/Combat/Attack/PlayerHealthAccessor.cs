using App.Player.ECS;
using Unity.Entities;
using Unity.Mathematics;

namespace App.Combat.Attack
{
    public class PlayerHealthAccessor : IHealthAccessor
    {
        public bool TryApplyDamage(Entity entity, float damage)
        {
            if (entity == Entity.Null) return false;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!em.Exists(entity) || !em.HasComponent<PlayerHealth>(entity))
                return false;

            var health = em.GetComponentData<PlayerHealth>(entity);
            health.Value = math.max(health.Value - damage, 0f);
            em.SetComponentData(entity, health);
            return true;
        }
    }
}
