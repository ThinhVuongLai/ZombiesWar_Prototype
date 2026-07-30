using R3;
using UnityEngine;

namespace App.JoystickInput
{
    internal sealed class JoystickInputManager : IJoystickInputManager
    {
        public ReadOnlyReactiveProperty<Vector2> Direction { get; }
        public ReadOnlyReactiveProperty<float> Magnitude { get; }
        public ReadOnlyReactiveProperty<bool> IsActive { get; }

        public JoystickInputManager(JoystickInputModel model)
        {
            Direction = model.Direction.ToReadOnlyReactiveProperty();
            Magnitude = model.Magnitude.ToReadOnlyReactiveProperty();
            IsActive = model.IsActive.ToReadOnlyReactiveProperty();
        }
    }
}
