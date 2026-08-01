using System;
using App.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App.JoystickInput
{
    public class JoystickInputView : MonoBehaviour, IJoystickInputView, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform _backgroundRectTransform;
        [SerializeField] RectTransform _handleRectTransform;
        [SerializeField] float _maximumRadius = 100f;

        public float MaximumRadius => _maximumRadius;

        public Action<Vector2> OnDragStarted { get; set; }
        public Action<Vector2> OnDragUpdated { get; set; }
        public Action OnDragEnded { get; set; }

        JoystickInputPresenter _presenter;

        void Awake()
        {
            EnsureEventSystem();
            var model = ServiceLocator.Resolve<JoystickInputModel>();
            _presenter = new JoystickInputPresenter(model, this);
        }

        void OnDestroy()
        {
            _presenter?.Dispose();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 input = GetLocalInput(eventData);
            OnDragStarted?.Invoke(input);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 input = GetLocalInput(eventData);
            OnDragUpdated?.Invoke(input);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnDragEnded?.Invoke();
        }

        Vector2 GetLocalInput(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _backgroundRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            return Vector2.ClampMagnitude(localPoint, _maximumRadius);
        }

        public void SetHandlePosition(Vector2 localPosition)
        {
            _handleRectTransform.anchoredPosition = localPosition;
        }

        public void SetActive(bool active)
        {
            _backgroundRectTransform.gameObject.SetActive(active);
        }

        public void ResetToCenter()
        {
            _handleRectTransform.anchoredPosition = Vector2.zero;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var gameObject = new GameObject("EventSystem");
                gameObject.AddComponent<EventSystem>();
                gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
