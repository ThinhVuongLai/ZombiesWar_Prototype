using System.Collections;
using System.Collections.Generic;
using App.Core.Services;
using App.Level;
using App.UI;

public class LosePopupPresenter : ICanvasPresenter
{
    private LosePopupView _view;
    private LevelController _levelController;
    private CanvasManager _canvasManager;

    public LosePopupPresenter(LosePopupView losePopupView)
    {
        _view = losePopupView;
        _levelController = ServiceLocator.Resolve<LevelController>();
        _canvasManager = ServiceLocator.Resolve<CanvasManager>();
    }

    public void Hide()
    {
        _view.ClickReplayButtonAction -= OnClickReplayButton;
        _view.ClickBackToMainButtonAction -= OnClickBackToMainButton;
    }

    public void Init(params object[] parameters)
    {
        _view.ClickReplayButtonAction += OnClickReplayButton;
        _view.ClickBackToMainButtonAction += OnClickBackToMainButton;
    }

    private void OnClickReplayButton()
    {
        if (_levelController)
        {
            _canvasManager.Hide(UIName.LosePopup);
            _levelController.ResetLevel();
        }
    }

    private void OnClickBackToMainButton()
    {
        _canvasManager.Hide(UIName.LosePopup);
        _canvasManager.Hide(UIName.InGameMenu);
        _levelController.ClearLevel();
        _canvasManager.Spawn(UIName.MainMenu);
    }
}
