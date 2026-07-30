using App.Core.EventBus;
using App.Core.Services;
using UnityEngine;

namespace App.Enemy.Attack
{
    public class EnemyMeleeAttack : IEnemyAttackStrategy
    {
        public EnemyAttackType AttackType => EnemyAttackType.Melee;
        public float Range => 2f;

        public void Execute(IEnemyView view, IPlayerTargetProvider target, float damage)
        {
            FacePlayer(view, target);

            var eventBus = ServiceLocator.Resolve<IEventBus>();
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
