using App.Core;
using App.Core.EventBus;
using App.Core.Services;
using Unity.Mathematics;
using UnityEngine;
using ZombiesWar.Bullet;
using ZombiesWar.Bullet.ECS;

namespace App.Enemy.Attack
{
    public class EnemyRangedAttack : IEnemyAttackStrategy
    {
        readonly BulletConfig _bulletConfig;
        readonly float _bulletDamage;

        public WeaponType AttackType => WeaponType.Range;

        public EnemyRangedAttack(BulletConfig bulletConfig, float bulletDamage)
        {
            _bulletConfig = bulletConfig;
            _bulletDamage = bulletDamage;
        }

        public void Execute(IEnemyView view, IPlayerTargetProvider target, float damage)
        {
            FacePlayer(view, target);

            if (_bulletConfig != null && PlayerTargetECSUpdater.PlayerEntity != Unity.Entities.Entity.Null)
            {
                var firePos = (float3)view.Transform.position + new float3(0, 1.5f, 0);
                BulletSpawner.SpawnBullet(_bulletConfig, _bulletDamage, firePos, PlayerTargetECSUpdater.PlayerEntity);
            }
            else
            {
                var eventBus = ServiceLocator.Resolve<IEventBus>();
                eventBus.Publish(new EnemyDealtDamageMessage(damage, WeaponType.Range));
            }
        }

        static void FacePlayer(IEnemyView view, IPlayerTargetProvider target)
        {
            var dir = (target.PlayerTransform.position - view.Transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
                view.Transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
