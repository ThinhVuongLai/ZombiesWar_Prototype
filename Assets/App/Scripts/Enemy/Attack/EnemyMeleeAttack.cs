using App.Core;
using App.Player.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace App.Enemy.Attack
{
    public class EnemyMeleeAttack : IEnemyAttackStrategy
    {
        public EnemyAttackType AttackType => EnemyAttackType.Melee;

        public void Execute(IEnemyView view, IPlayerTargetProvider target, float damage)
        {
            FacePlayer(view, target);

            var playerEntity = PlayerTargetECSUpdater.PlayerEntity;
            if (playerEntity != Entity.Null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (em.Exists(playerEntity) && em.HasComponent<PlayerHealth>(playerEntity))
                {
                    var health = em.GetComponentData<PlayerHealth>(playerEntity);
                    health.Value = math.max(health.Value - damage, 0f);
                    em.SetComponentData(playerEntity, health);
                    return;
                }
            }

            var eventBus = Core.Services.ServiceLocator.Resolve<Core.EventBus.IEventBus>();
            eventBus.Publish(new EnemyDealtDamageMessage(damage, EnemyAttackType.Melee));
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
