using R3;
using UnityEngine;

namespace App.Player
{
    public class PlayerModel
    {
        public ReactiveProperty<float> MoveSpeed { get; } = new(5f);
        public ReactiveProperty<bool> IsAlive { get; } = new(true);
        public ReactiveProperty<PlayerStateType> CurrentState { get; } = new(PlayerStateType.Idle);
        public ReactiveProperty<float> Health { get; } = new(100f);
        public ReactiveProperty<float> MaxHealth { get; } = new(100f);
    }

    public enum PlayerStateType
    {
        Idle,
        Move,
        Die
    }
}
