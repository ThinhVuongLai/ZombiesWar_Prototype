using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class InGameMenuView : CanvasBase
    {
        [SerializeField] private TextMeshProUGUI _remainingEnemyText;
        [SerializeField] private Button _backToMainButton;
        [SerializeField] private Button _useRocketBoosterButton;
        [SerializeField] private Button _changeWeaponButton;

        public Action ClickBackToMainAction;
        public Action ClickUseRocketBoosterAction;
        public Action ClickChangeWeaponAction;

        private void Awake()
        {
            _backToMainButton.onClick.AddListener(() => ClickBackToMainAction?.Invoke());
            _useRocketBoosterButton.onClick.AddListener(() => ClickUseRocketBoosterAction?.Invoke());
            _changeWeaponButton.onClick.AddListener(() => ClickChangeWeaponAction?.Invoke());
        }

        public override ICanvasPresenter FirstSpawn()
            => new InGameMenuPresenter(this);

        public void SetRemainingEnemyText(string text)
            => _remainingEnemyText.text = text;
    }
}
