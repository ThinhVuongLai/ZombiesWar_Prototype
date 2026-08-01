using System;
using UnityEngine;

namespace App.JoystickInput
{
    public interface IJoystickInputView
    {
        float MaximumRadius { get; }
        Action<Vector2> OnDragStarted { get; set; }
        Action<Vector2> OnDragUpdated { get; set; }
        Action OnDragEnded { get; set; }
        void SetHandlePosition(Vector2 localPosition);
        void SetActive(bool active);
        void ResetToCenter();
    }
}
