using App.Player;

namespace App.Player.States
{
    public interface IPlayerState
    {
        PlayerStateType StateType { get; }
        void Enter(PlayerPresenter presenter);
        void Update(PlayerPresenter presenter);
        void Exit(PlayerPresenter presenter);
    }
}
