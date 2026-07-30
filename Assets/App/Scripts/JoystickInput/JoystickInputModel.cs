using R3;
using UnityEngine;

namespace App.JoystickInput
{
    public class JoystickInputModel
    {
        public ReactiveProperty<Vector2> Direction { get; } = new(Vector2.zero);
        public ReactiveProperty<float> Magnitude { get; } = new(0f);
        public ReactiveProperty<bool> IsActive { get; } = new(false);
    }
}
