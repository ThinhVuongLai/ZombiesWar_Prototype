using System;
using Unity.Entities;
using UnityEngine;

namespace App.Combat.Attack
{
    public interface IAttackStrategy
    {
        WeaponType AttackType { get; }
        void Execute(Vector3 attackerPos, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPos, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null);
    }
}
