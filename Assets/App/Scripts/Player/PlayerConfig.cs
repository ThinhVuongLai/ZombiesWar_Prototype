using System;
using UnityEngine;

namespace App.Player
{
    [Serializable]
    public class PlayerInfor
    {
        [SerializeField] int _playerId;
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] float _health;
        [SerializeField] string _idleAnimation;
        [SerializeField] string _moveAnimation;
        [SerializeField] string _deadAnimation;

        public int PlayerId => _playerId;
        public GameObject PlayerPrefab => _playerPrefab;
        public float Health => _health;
        public string IdleAnimation => _idleAnimation;
        public string MoveAnimation => _moveAnimation;
        public string DeadAnimation => _deadAnimation;
    }

    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ZombiesWar/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] int _moveAnimationLayerIndex;
        [SerializeField] int _attackAnimationLayerIndex;
        [SerializeField] PlayerInfor[] _playerInfor;

        public int MoveAnimationLayerIndex => _moveAnimationLayerIndex;
        public int AttackAnimationLayerIndex => _attackAnimationLayerIndex;
        public PlayerInfor[] PlayerInfor => _playerInfor;
    }
}
