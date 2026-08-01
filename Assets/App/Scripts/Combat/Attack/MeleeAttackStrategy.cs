using System;
using Unity.Entities;
using UnityEngine;

namespace App.Combat.Attack
{
    public class MeleeAttackStrategy : IAttackStrategy
    {
        public WeaponType AttackType => WeaponType.Melee;

        public void Execute(Vector3 attackerPosition, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPosition, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (faceTarget && attackerTransform != null)
                FaceTarget(attackerTransform, targetPosition);

            if (!healthAccessor.TryApplyDamage(targetEntity, damage))
                fallbackDamageDealer?.Invoke(damage);
        }

        static void FaceTarget(Transform transform, Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
