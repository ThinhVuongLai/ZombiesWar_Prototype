using App.Player;
using UnityEngine;

namespace App.Player.States
{
    public class PlayerStateIdle : IPlayerState
    {
        const float InputThreshold = 0.01f;

        public PlayerStateType StateType => PlayerStateType.Idle;

        public void Enter(PlayerPresenter presenter)
        {
            presenter.VerticalVelocity = 0f;
        }

        public void Update(PlayerPresenter presenter)
        {
            ApplyGravity(presenter);

            if (presenter.HasCombatTarget)
            {
                presenter.View.FaceDirection(presenter.CombatTargetDirection);
            }

            if (presenter.Input.Magnitude.CurrentValue > InputThreshold)
            {
                presenter.TransitionTo(PlayerStateType.Move);
            }
        }

        public void Exit(PlayerPresenter presenter) { }

        static void ApplyGravity(PlayerPresenter presenter)
        {
            if (presenter.View.IsGrounded && presenter.VerticalVelocity < 0f)
            {
                presenter.VerticalVelocity = -1f;
            }
            else
            {
                presenter.VerticalVelocity += PlayerPresenter.Gravity * Time.deltaTime;
            }

            Vector3 motion = Vector3.up * (presenter.VerticalVelocity * Time.deltaTime);
            presenter.View.Move(motion);
        }
    }
}
