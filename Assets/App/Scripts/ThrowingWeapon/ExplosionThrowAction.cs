using UnityEngine;

namespace ZombiesWar.ThrowingWeapon
{
    public class ExplosionThrowAction : IThrowAction
    {
        public ThrowActionType ActionType => ThrowActionType.Explosion;

        public void Execute(Vector3 position, float radius, float damage)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }
}
