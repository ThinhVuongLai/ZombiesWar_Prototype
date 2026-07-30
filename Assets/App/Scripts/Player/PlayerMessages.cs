namespace App.Player
{
    public readonly struct PlayerStateUpdatedMessage
    {
        public readonly PlayerStateType StateType;

        public PlayerStateUpdatedMessage(PlayerStateType stateType)
        {
            StateType = stateType;
        }
    }
}
