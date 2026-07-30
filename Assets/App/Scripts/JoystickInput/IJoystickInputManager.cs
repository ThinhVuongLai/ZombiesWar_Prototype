using R3;
using UnityEngine;

namespace App.JoystickInput
{
    public interface IJoystickInputManager
    {
        ReadOnlyReactiveProperty<Vector2> Direction { get; }
        ReadOnlyReactiveProperty<float> Magnitude { get; }
        ReadOnlyReactiveProperty<bool> IsActive { get; }
    }
}
