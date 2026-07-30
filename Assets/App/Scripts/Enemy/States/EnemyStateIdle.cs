using App.Enemy;

namespace App.Enemy.States
{
    public class EnemyStateIdle : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Idle;

        public void Enter(EnemyPresenter presenter)
        {
            presenter.View.StopMovement();
        }

        public void Update(EnemyPresenter presenter)
        {
            if (presenter.CachedDetectionState != EnemyDetectionState.None)
                presenter.TransitionTo(EnemyStateType.Move);
        }

        public void Exit(EnemyPresenter presenter) { }
    }
}