using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ZombiesWar.Bullet.ECS
{
    public static class BulletSpawner
    {
        public static Entity SpawnBullet(BulletConfig config, float3 startPosition, Entity targetEntity)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var em = world.EntityManager;

            float lifetime = config.Speed > 0f ? config.Range / config.Speed : 0f;

            var entity = em.CreateEntity(
                typeof(BulletData),
                typeof(BulletLifeData),
                typeof(LocalTransform)
            );

            em.SetComponentData(entity, new BulletData
            {
                Speed = config.Speed,
                Damage = config.Damage,
                Lifetime = lifetime,
                MovementType = config.MovementType,
                TargetEntity = targetEntity,
            });

            em.SetComponentData(entity, new BulletLifeData
            {
                RemainingLife = lifetime,
                StartPosition = startPosition,
                PreviousPosition = startPosition,
                HasHit = false,
            });

            em.SetComponentData(entity, LocalTransform.FromPosition(startPosition));

            return entity;
        }
    }
}
