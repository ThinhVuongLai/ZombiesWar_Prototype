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

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.Exists(entity) || !entityManager.HasComponent<PlayerHealth>(entity))
                return false;

            var health = entityManager.GetComponentData<PlayerHealth>(entity);
            health.Value = math.max(health.Value - damage, 0f);
            entityManager.SetComponentData(entity, health);
            return true;
        }
    }
}
