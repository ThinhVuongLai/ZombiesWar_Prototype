using App.JoystickInput;
using R3;
using UnityEngine;

namespace App.Player
{
    internal sealed class JoystickPlayerInputAdapter : IPlayerInputProvider
    {
        public ReadOnlyReactiveProperty<Vector2> Direction { get; }
        public ReadOnlyReactiveProperty<float> Magnitude { get; }

        public JoystickPlayerInputAdapter(IJoystickInputManager joystick)
        {
            Direction = joystick.Direction;
            Magnitude = joystick.Magnitude;
        }
    }
}
