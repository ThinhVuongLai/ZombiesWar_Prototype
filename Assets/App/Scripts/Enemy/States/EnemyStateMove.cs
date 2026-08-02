using System.Diagnostics;
using App.Enemy;

namespace App.Enemy.States
{
    public class EnemyStateMove : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Move;

        public void Enter(EnemyPresenter presenter)
        {
            presenter.View.SetAgentEnabled(true);
        }

        public void Update(EnemyPresenter presenter)
        {
            var detectionState = presenter.CachedDetectionState;

            if (detectionState == EnemyDetectionState.InAttackRange)
            {
                presenter.TransitionTo(EnemyStateType.Attack);
                return;
            }

            if (detectionState == EnemyDetectionState.None)
            {
                presenter.TransitionTo(EnemyStateType.Idle);
                return;
            }

            presenter.View.SetDestination(presenter.PlayerTarget.PlayerTransform.position);
        }

        public void Exit(EnemyPresenter presenter) { }
    }
}