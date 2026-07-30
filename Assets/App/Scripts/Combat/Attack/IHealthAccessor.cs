using Unity.Entities;

namespace App.Combat.Attack
{
    public interface IHealthAccessor
    {
        bool TryApplyDamage(Entity entity, float damage);
    }
}
