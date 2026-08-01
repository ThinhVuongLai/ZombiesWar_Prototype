using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Bullet.ECS;

namespace App.Combat.Attack
{
    public class RangedAttackStrategy : IAttackStrategy
    {
        readonly BulletConfig _bulletConfig;
        readonly float _bulletDamage;

        public WeaponType AttackType => WeaponType.Range;

        public RangedAttackStrategy(BulletConfig bulletConfig, float bulletDamage)
        {
            _bulletConfig = bulletConfig;
            _bulletDamage = bulletDamage;
        }

        public void Execute(Vector3 attackerPosition, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPosition, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (faceTarget && attackerTransform != null)
                FaceTarget(attackerTransform, targetPosition);

            if (_bulletConfig != null && targetEntity != Entity.Null)
            {
                var firePosition = (float3)attackerPosition + new float3(0, 1.5f, 0);
                BulletSpawner.SpawnBullet(_bulletConfig, _bulletDamage, firePosition, targetEntity);
            }
            else
            {
                fallbackDamageDealer?.Invoke(damage);
            }
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
