using App.Player;

namespace App.Player.States
{
    public class PlayerStateDie : IPlayerState
    {
        public PlayerStateType StateType => PlayerStateType.Die;

        public void Enter(PlayerPresenter presenter)
        {
            // Placeholder: disable movement, play death animation later
        }

        public void Update(PlayerPresenter presenter)
        {
            // No movement when dead
        }

        public void Exit(PlayerPresenter presenter)
        {
            // Placeholder: respawn logic later
        }
    }
}
