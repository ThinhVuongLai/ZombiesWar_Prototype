using R3;

namespace App.HealthBar
{
    public class HealthBarPresenter
    {
        readonly IHealthBarView _view;
        readonly float _maximumHealth;
        readonly CompositeDisposable _disposables = new();

        private bool _isActive = false;

        public HealthBarPresenter(IHealthBarView view, ReactiveProperty<float> health, float maximumHealth)
        {
            _view = view;
            _maximumHealth = maximumHealth;

            health.Subscribe(OnHealthChanged).AddTo(_disposables);

            _isActive = true;
        }

        void OnHealthChanged(float current)
        {
            if (current <= 0f)
            {
                _view.SetVisible(false);
                return;
            }

            _view.SetVisible(true);
            _view.SetFillAmount(current / _maximumHealth);
        }

        public void Dispose()
        {
            if (!_isActive)
                return;

            _disposables.Dispose();
            if (_view is UnityEngine.MonoBehaviour viewBehaviour)
            {
                _isActive = false;
                UnityEngine.Object.Destroy(viewBehaviour.gameObject);
            }
        }
    }
}
