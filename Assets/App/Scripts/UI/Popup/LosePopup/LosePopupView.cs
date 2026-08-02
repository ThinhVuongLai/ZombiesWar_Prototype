using System.Collections;
using System.Collections.Generic;
using App.UI;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LosePopupView : CanvasBase
{
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _backToMainButton;

    public Action ClickReplayButtonAction = null;
    public Action ClickBackToMainButtonAction = null;

    private void Awake()
    {
        _replayButton.onClick.AddListener(()=>
        {
            ClickReplayButtonAction?.Invoke();
        });

        _backToMainButton.onClick.AddListener(()=>
        {
           ClickBackToMainButtonAction?.Invoke(); 
        });
    }


    public override ICanvasPresenter FirstSpawn()
    {
        LosePopupPresenter losePopupPresenter = new LosePopupPresenter(this);

        if (losePopupPresenter is ICanvasPresenter presenter)
        {
            return presenter;
        }
        else
        {
            return null;
        }
    }
}
