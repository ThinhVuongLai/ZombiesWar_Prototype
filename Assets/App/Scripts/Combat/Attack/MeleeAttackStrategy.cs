using System;
using Unity.Entities;
using UnityEngine;

namespace App.Combat.Attack
{
    public class MeleeAttackStrategy : IAttackStrategy
    {
        public WeaponType AttackType => WeaponType.Melee;

        public void Execute(Vector3 attackerPos, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPos, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (faceTarget && attackerTransform != null)
                FaceTarget(attackerTransform, targetPos);

            if (!healthAccessor.TryApplyDamage(targetEntity, damage))
                fallbackDamageDealer?.Invoke(damage);
        }

        static void FaceTarget(Transform transform, Vector3 targetPos)
        {
            var dir = (targetPos - transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
