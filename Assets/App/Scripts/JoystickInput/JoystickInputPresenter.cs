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

        void HandleDragStarted(Vector2 clampedLocalPos)
        {
            _model.IsActive.Value = true;
            ProcessInput(clampedLocalPos);
        }

        void HandleDragUpdated(Vector2 clampedLocalPos)
        {
            ProcessInput(clampedLocalPos);
        }

        void HandleDragEnded()
        {
            _model.IsActive.Value = false;
            _model.Direction.Value = Vector2.zero;
            _model.Magnitude.Value = 0f;
            _view.ResetToCenter();
        }

        void ProcessInput(Vector2 clampedLocalPos)
        {
            float magnitude = clampedLocalPos.magnitude / _view.MaxRadius;
            _view.SetHandlePosition(clampedLocalPos);
            _model.Direction.Value = clampedLocalPos.normalized;
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
