using App.Enemy;

namespace App.Enemy.States
{
    public interface IEnemyState
    {
        EnemyStateType StateType { get; }
        void Enter(EnemyPresenter presenter);
        void Update(EnemyPresenter presenter);
        void Exit(EnemyPresenter presenter);
    }
}
