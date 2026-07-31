using App.Enemy;
using UnityEngine;

namespace App.Enemy.States
{
    public class EnemyStateDie : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Die;

        public void Enter(EnemyPresenter presenter)
        {
            presenter.View.SetAgentEnabled(false);
            presenter.DestroyECSCombatState();
        }

        public void Update(EnemyPresenter presenter)
        {
            Debug.Log("in Die");
        }

        public void Exit(EnemyPresenter presenter) { }
    }
}