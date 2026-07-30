using R3;

namespace App.HealthBar
{
    public class HealthBarPresenter
    {
        readonly IHealthBarView _view;
        readonly float _maxHealth;
        readonly CompositeDisposable _disposables = new();

        public HealthBarPresenter(IHealthBarView view, ReactiveProperty<float> health, float maxHealth)
        {
            _view = view;
            _maxHealth = maxHealth;

            health.Subscribe(OnHealthChanged).AddTo(_disposables);
        }

        void OnHealthChanged(float current)
        {
            if (current <= 0f)
            {
                _view.SetVisible(false);
                return;
            }

            _view.SetVisible(true);
            _view.SetFillAmount(current / _maxHealth);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
