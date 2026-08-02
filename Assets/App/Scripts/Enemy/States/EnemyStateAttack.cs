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
            presenter.ProcessAttackDuration();

            if (presenter.IsInAttackDuration) return;

            var detectionState = presenter.CachedDetectionState;

            switch (detectionState)
            {
                case EnemyDetectionState.InDetectionRange:
                    {
                        presenter.TransitionTo(EnemyStateType.Move);
                    }
                    break;
                case EnemyDetectionState.InAttackRange:
                    {
                        presenter.TryAttack(UnityEngine.Time.time);
                    }
                    break;
                default:
                    {
                        presenter.TransitionTo(EnemyStateType.Idle);
                    }
                    break;
            }
        }

        public void Exit(EnemyPresenter presenter) { }
    }
}