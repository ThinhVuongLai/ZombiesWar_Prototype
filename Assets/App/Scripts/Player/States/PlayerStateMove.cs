using App.Player;
using UnityEngine;

namespace App.Player.States
{
    public class PlayerStateMove : IPlayerState
    {
        const float InputThreshold = 0.01f;

        public PlayerStateType StateType => PlayerStateType.Move;

        public void Enter(PlayerPresenter presenter) { }

        public void Update(PlayerPresenter presenter)
        {
            float magnitude = presenter.Input.Magnitude.CurrentValue;
            Vector2 direction = presenter.Input.Direction.CurrentValue;

            if (magnitude <= InputThreshold)
            {
                presenter.TransitionTo(PlayerStateType.Idle);
                return;
            }

            Vector3 worldDir = new Vector3(direction.x, 0f, direction.y);
            Vector3 horizontalMotion = worldDir * (magnitude * presenter.MoveSpeed * Time.deltaTime);

            ApplyGravity(presenter);

            Vector3 verticalMotion = Vector3.up * (presenter.VerticalVelocity * Time.deltaTime);
            presenter.View.Move(horizontalMotion + verticalMotion);

            Vector3 faceDirection = presenter.HasCombatTarget
                ? presenter.CombatTargetDirection
                : worldDir;
            presenter.View.FaceDirection(faceDirection);
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
        }
    }
}
