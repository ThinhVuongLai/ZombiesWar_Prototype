using App.Core.Services;
using R3;
using System;

namespace App.UI
{
    public class LoadingPopupPresenter : ICanvasPresenter
    {
        private readonly LoadingPopupView _view;
        private CompositeDisposable _disposables;

        public LoadingPopupPresenter(LoadingPopupView view)
        {
            _view = view;
        }

        public void Init(params object[] parameters)
        {
            _disposables = new CompositeDisposable();

            Observable.Timer(TimeSpan.FromSeconds(1f))
                .Subscribe(_ =>
                {
                    ServiceLocator.Resolve<CanvasManager>().Hide(UIName.LoadingPopup);
                })
                .AddTo(_disposables);
        }

        public void Hide()
        {
            _disposables?.Dispose();
        }
    }
}
