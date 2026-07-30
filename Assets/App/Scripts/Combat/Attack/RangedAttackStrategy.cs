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

        public void Execute(Vector3 attackerPos, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPos, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (faceTarget && attackerTransform != null)
                FaceTarget(attackerTransform, targetPos);

            if (_bulletConfig != null && targetEntity != Entity.Null)
            {
                var firePos = (float3)attackerPos + new float3(0, 1.5f, 0);
                BulletSpawner.SpawnBullet(_bulletConfig, _bulletDamage, firePos, targetEntity);
            }
            else
            {
                fallbackDamageDealer?.Invoke(damage);
            }
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
