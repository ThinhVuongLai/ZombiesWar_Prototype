using System;
using App.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App.JoystickInput
{
    public class JoystickInputView : MonoBehaviour, IJoystickInputView, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform _backgroundRT;
        [SerializeField] RectTransform _handleRT;
        [SerializeField] float _maxRadius = 100f;

        public float MaxRadius => _maxRadius;

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
                _backgroundRT, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            return Vector2.ClampMagnitude(localPoint, _maxRadius);
        }

        public void SetHandlePosition(Vector2 localPosition)
        {
            _handleRT.anchoredPosition = localPosition;
        }

        public void SetActive(bool active)
        {
            _backgroundRT.gameObject.SetActive(active);
        }

        public void ResetToCenter()
        {
            _handleRT.anchoredPosition = Vector2.zero;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
