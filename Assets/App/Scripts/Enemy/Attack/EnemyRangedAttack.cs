using App.Core.EventBus;
using App.Core.Services;
using UnityEngine;

namespace App.Enemy.Attack
{
    public class EnemyRangedAttack : IEnemyAttackStrategy
    {
        public EnemyAttackType AttackType => EnemyAttackType.Ranged;
        public float Range => 8f;

        public void Execute(IEnemyView view, IPlayerTargetProvider target, float damage)
        {
            FacePlayer(view, target);

            var eventBus = ServiceLocator.Resolve<IEventBus>();
            eventBus.Publish(new EnemyDealtDamageMessage(damage, EnemyAttackType.Ranged));
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
