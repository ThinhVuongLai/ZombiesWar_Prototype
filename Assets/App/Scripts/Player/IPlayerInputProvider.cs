using R3;
using UnityEngine;

namespace App.Player
{
    public interface IPlayerInputProvider
    {
        ReadOnlyReactiveProperty<Vector2> Direction { get; }
        ReadOnlyReactiveProperty<float> Magnitude { get; }
    }
}
