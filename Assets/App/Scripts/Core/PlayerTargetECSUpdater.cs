using App.Player.ECS;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace App.Core
{
    public class PlayerTargetECSUpdater : MonoBehaviour
    {
        Entity _singletonEntity;
        Entity _playerEntity;
        EntityManager _em;
        IPlayerTargetProvider _playerTarget;

        public static Entity PlayerEntity { get; private set; }

        public void Initialize(IPlayerTargetProvider playerTarget)
        {
            _playerTarget = playerTarget;
            _em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var query = _em.CreateEntityQuery(typeof(PlayerTargetECSData));
            _singletonEntity = query.GetSingletonEntity();

            var playerQuery = _em.CreateEntityQuery(typeof(PlayerHealth), typeof(LocalTransform));
            _playerEntity = playerQuery.GetSingletonEntity();
            PlayerEntity = _playerEntity;
        }

        void Update()
        {
            if (_playerTarget == null || _singletonEntity == Entity.Null)
                return;

            var pos = (float3)_playerTarget.PlayerTransform.position;

            _em.SetComponentData(_singletonEntity, new PlayerTargetECSData
            {
                Position = pos,
                IsAlive = _playerTarget.IsAlive,
            });

            if (_playerEntity != Entity.Null && _em.Exists(_playerEntity))
            {
                _em.SetComponentData(_playerEntity, LocalTransform.FromPosition(pos));
            }
        }
    }
}
