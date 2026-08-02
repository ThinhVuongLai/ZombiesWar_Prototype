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

        [Header("For Rocket")]
        [SerializeField] private Image _fillRocketImage;

        [Header("Start Level Object")]
        [SerializeField] private GameObject _startLevelObject;
        [SerializeField] private Button _startLevelButton;

        public Action ClickBackToMainAction;
        public Action ClickUseRocketBoosterAction;
        public Action ClickChangeWeaponAction;
        public Action ClickStartLevelAction;

        private void Awake()
        {
            _backToMainButton.onClick.AddListener(() => ClickBackToMainAction?.Invoke());
            _useRocketBoosterButton.onClick.AddListener(() => ClickUseRocketBoosterAction?.Invoke());
            _changeWeaponButton.onClick.AddListener(() => ClickChangeWeaponAction?.Invoke());
            _startLevelButton.onClick.AddListener(()=>ClickStartLevelAction?.Invoke());
        }

        public override ICanvasPresenter FirstSpawn()
            => new InGameMenuPresenter(this);

        public void SetRemainingEnemyText(string text)
            => _remainingEnemyText.text = text;

        public void SetFillRocket(float percent)
        {
            percent = Mathf.Clamp01(1 - percent);

            _fillRocketImage.fillAmount = percent;

            if (percent <= 0)
            {
                SetShowRocketFill(false);
            }
        }

        public void SetShowRocketFill(bool show)
        {
            _fillRocketImage.gameObject.SetActive(show);

            _useRocketBoosterButton.interactable = !show;
        }

        public void SetShowStartLevelObject(bool show)
        {
            _startLevelObject.SetActive(show);
        }
    }
}
