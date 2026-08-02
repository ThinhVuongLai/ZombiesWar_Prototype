using System;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class LevelItem : MonoBehaviour
    {
        [SerializeField] private Button _button;

        public int LevelId { get; private set; }
        public Action<int> ClickAction;

        private void Awake()
        {
            _button.onClick.AddListener(() => ClickAction?.Invoke(LevelId));
        }

        public void Init(int levelId)
        {
            LevelId = levelId;
        }
    }
}
