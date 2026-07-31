using UnityEngine;

namespace App.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ZombiesWar/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] int _moveAnimationLayerIndex;
        [SerializeField] int _attackAnimationLayerIndex;
        [SerializeField] string _idleAnimation;
        [SerializeField] string _moveAnimation;

        public int MoveAnimationLayerIndex => _moveAnimationLayerIndex;
        public int AttackAnimationLayerIndex => _attackAnimationLayerIndex;
        public string IdleAnimation => _idleAnimation;
        public string MoveAnimation => _moveAnimation;
    }
}
