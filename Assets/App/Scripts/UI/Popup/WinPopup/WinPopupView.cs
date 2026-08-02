using System;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class WinPopupView : CanvasBase
    {
        [SerializeField] private Button _backToMainMenuButton;

        public Action ClickBackToMainMenuAction;

        private void Awake()
        {
            _backToMainMenuButton.onClick.AddListener(() => ClickBackToMainMenuAction?.Invoke());
        }

        public override ICanvasPresenter FirstSpawn()
            => new WinPopupPresenter(this);
    }
}
