using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace App.Core
{
    public class PlayerTargetECSUpdater : MonoBehaviour
    {
        Entity _singletonEntity;
        EntityManager _em;
        IPlayerTargetProvider _playerTarget;

        public void Initialize(IPlayerTargetProvider playerTarget)
        {
            _playerTarget = playerTarget;
            _em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var query = _em.CreateEntityQuery(typeof(PlayerTargetECSData));
            _singletonEntity = query.GetSingletonEntity();
        }

        void Update()
        {
            if (_playerTarget == null || _singletonEntity == Entity.Null)
                return;

            _em.SetComponentData(_singletonEntity, new PlayerTargetECSData
            {
                Position = (float3)_playerTarget.PlayerTransform.position,
                IsAlive = _playerTarget.IsAlive,
            });
        }
    }
}
