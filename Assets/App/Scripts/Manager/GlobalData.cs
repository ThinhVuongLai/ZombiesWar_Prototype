using UnityEngine;

namespace App.Core
{
    [CreateAssetMenu(fileName = "GlobalData", menuName = "ZombiesWar/Global Data")]
    public class GlobalData : ScriptableObject
    {
        [Header("Damage Flash")]
        [SerializeField] Color _damageFlashColor = Color.red;
        [SerializeField] float _damageFlashDuration = 0.3f;

        public Color DamageFlashColor => _damageFlashColor;
        public float DamageFlashDuration => _damageFlashDuration;
    }
}
