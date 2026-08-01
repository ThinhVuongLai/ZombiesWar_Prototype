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
        EntityManager _entityManager;
        IPlayerTargetProvider _playerTarget;

        public static Entity PlayerEntity { get; private set; }

        public void Initialize(IPlayerTargetProvider playerTarget)
        {
            _playerTarget = playerTarget;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var query = _entityManager.CreateEntityQuery(typeof(PlayerTargetECSData));
            _singletonEntity = query.GetSingletonEntity();

            var playerQuery = _entityManager.CreateEntityQuery(typeof(PlayerHealth), typeof(LocalTransform));
            _playerEntity = playerQuery.GetSingletonEntity();
            PlayerEntity = _playerEntity;
        }

        void Update()
        {
            if (_playerTarget == null || _singletonEntity == Entity.Null)
                return;

            var position = (float3)_playerTarget.PlayerTransform.position;

            _entityManager.SetComponentData(_singletonEntity, new PlayerTargetECSData
            {
                Position = position,
                IsAlive = _playerTarget.IsAlive,
            });

            if (_playerEntity != Entity.Null && _entityManager.Exists(_playerEntity))
            {
                _entityManager.SetComponentData(_playerEntity, LocalTransform.FromPosition(position));
            }
        }
    }
}
