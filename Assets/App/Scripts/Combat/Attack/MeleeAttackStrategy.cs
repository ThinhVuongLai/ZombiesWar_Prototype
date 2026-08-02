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

        public static bool IsTargetInHitZone(
            Vector3 attackerPosition, Vector3 attackerForward,
            Vector3 targetPosition, Vector2 hitZoneSize)
        {
            var toTarget = targetPosition - attackerPosition;
            toTarget.y = 0f;

            var forwardDistance = Vector3.Dot(toTarget, attackerForward);
            if (forwardDistance < 0f || forwardDistance > hitZoneSize.y)
                return false;

            var attackerRight = Vector3.Cross(Vector3.up, attackerForward).normalized;
            var rightDistance = Mathf.Abs(Vector3.Dot(toTarget, attackerRight));
            return rightDistance <= hitZoneSize.x * 0.5f;
        }
    }
}
