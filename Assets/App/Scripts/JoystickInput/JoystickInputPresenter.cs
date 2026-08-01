using System;
using R3;
using UnityEngine;

namespace App.JoystickInput
{
    public class JoystickInputPresenter : IDisposable
    {
        readonly JoystickInputModel _model;
        readonly IJoystickInputView _view;
        readonly CompositeDisposable _disposables = new();

        public JoystickInputPresenter(JoystickInputModel model, IJoystickInputView view)
        {
            _model = model;
            _view = view;

            _view.OnDragStarted += HandleDragStarted;
            _view.OnDragUpdated += HandleDragUpdated;
            _view.OnDragEnded += HandleDragEnded;
        }

        void HandleDragStarted(Vector2 clampedLocalPosition)
        {
            _model.IsActive.Value = true;
            ProcessInput(clampedLocalPosition);
        }

        void HandleDragUpdated(Vector2 clampedLocalPosition)
        {
            ProcessInput(clampedLocalPosition);
        }

        void HandleDragEnded()
        {
            _model.IsActive.Value = false;
            _model.Direction.Value = Vector2.zero;
            _model.Magnitude.Value = 0f;
            _view.ResetToCenter();
        }

        void ProcessInput(Vector2 clampedLocalPosition)
        {
            float magnitude = clampedLocalPosition.magnitude / _view.MaximumRadius;
            _view.SetHandlePosition(clampedLocalPosition);
            _model.Direction.Value = clampedLocalPosition.normalized;
            _model.Magnitude.Value = Mathf.Clamp01(magnitude);
        }

        public void Dispose()
        {
            _view.OnDragStarted -= HandleDragStarted;
            _view.OnDragUpdated -= HandleDragUpdated;
            _view.OnDragEnded -= HandleDragEnded;
            _disposables.Dispose();
        }
    }
}
