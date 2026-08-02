using System.Collections.Generic;
using UnityEngine;

namespace App.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "ZombiesWar/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        [SerializeField] private List<UIInfo> _uiInfos;

        public List<UIInfo> UIInfos => _uiInfos;
    }
}
