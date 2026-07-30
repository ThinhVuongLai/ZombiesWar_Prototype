using App.Enemy;

namespace App.Enemy.States
{
    public class EnemyStateAttack : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Attack;

        public void Enter(EnemyPresenter presenter)
        {
            presenter.View.SetAgentEnabled(false);
        }

        public void Update(EnemyPresenter presenter)
        {
            var detectionState = presenter.CachedDetectionState;

            if (detectionState == EnemyDetectionState.InDetectionRange)
            {
                presenter.TransitionTo(EnemyStateType.Move);
                return;
            }

            if (detectionState == EnemyDetectionState.None)
            {
                presenter.TransitionTo(EnemyStateType.Idle);
                return;
            }

            if (presenter.CachedNeedsCombatResult)
            {
                presenter.ExecuteAttack();
                presenter.ResetNeedsCombatResult();
            }
        }

        public void Exit(EnemyPresenter presenter) { }
    }
}