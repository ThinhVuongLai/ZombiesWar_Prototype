using App.Core.Services;
using App.Level;

namespace App.UI
{
    public class WinPopupPresenter : ICanvasPresenter
    {
        private readonly WinPopupView _view;
        private readonly LevelController _levelController;
        private readonly CanvasManager _canvasManager;

        public WinPopupPresenter(WinPopupView view)
        {
            _view = view;
            _levelController = ServiceLocator.Resolve<LevelController>();
            _canvasManager = ServiceLocator.Resolve<CanvasManager>();
        }

        public void Init(params object[] parameters)
        {
            _view.ClickBackToMainMenuAction = OnClickBackToMainMenu;
        }

        public void Hide()
        {
            _view.ClickBackToMainMenuAction = null;
        }

        private void OnClickBackToMainMenu()
        {
            _canvasManager.Hide(UIName.InGameMenu);
            _canvasManager.Hide(UIName.WinPopup);
            _levelController.ClearLevel();
            _canvasManager.Spawn(UIName.MainMenu);
        }
    }
}
