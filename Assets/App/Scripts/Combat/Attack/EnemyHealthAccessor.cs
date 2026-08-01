using Unity.Entities;
using Unity.Mathematics;

namespace App.Combat.Attack
{
    public class EnemyHealthAccessor : IHealthAccessor
    {
        public bool TryApplyDamage(Entity entity, float damage)
        {
            if (entity == Entity.Null) return false;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.Exists(entity) || !entityManager.HasComponent<EnemyHealth>(entity))
                return false;

            var health = entityManager.GetComponentData<EnemyHealth>(entity);
            health.Value = math.max(health.Value - damage, 0f);
            entityManager.SetComponentData(entity, health);
            return true;
        }
    }
}
