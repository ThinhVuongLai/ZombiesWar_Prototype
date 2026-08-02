using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class MainMenuView : CanvasBase
    {
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private LevelItem _levelItemPrefab;

        public Action<int> ClickLevelItemAction;
        private readonly List<LevelItem> _spawnedItems = new();

        public override ICanvasPresenter FirstSpawn()
            => new MainMenuPresenter(this);

        public void SetLevelItems(List<int> levelIds)
        {
            foreach (var item in _spawnedItems)
                Destroy(item.gameObject);
            _spawnedItems.Clear();

            foreach (var levelId in levelIds)
            {
                var item = Instantiate(_levelItemPrefab, _gridLayoutGroup.transform);
                item.Init(levelId);
                item.ClickAction = id => ClickLevelItemAction?.Invoke(id);
                _spawnedItems.Add(item);
            }
        }
    }
}
