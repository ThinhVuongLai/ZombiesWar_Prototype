using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ZombiesWar.Bullet.ECS
{
    public static class BulletSpawner
    {
        public static Entity SpawnBullet(BulletConfig config, float damage, float3 startPosition, Entity targetEntity)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;

            float lifetime = config.Speed > 0f ? config.Range / config.Speed : 0f;

            var entity = entityManager.CreateEntity(
                typeof(BulletData),
                typeof(BulletLifeData),
                typeof(LocalTransform)
            );

            entityManager.SetComponentData(entity, new BulletData
            {
                Speed = config.Speed,
                Damage = damage,
                Lifetime = lifetime,
                MovementType = config.MovementType,
                TargetEntity = targetEntity,
            });

            entityManager.SetComponentData(entity, new BulletLifeData
            {
                RemainingLife = lifetime,
                StartPosition = startPosition,
                PreviousPosition = startPosition,
                HasHit = false,
            });

            entityManager.SetComponentData(entity, LocalTransform.FromPosition(startPosition));

            return entity;
        }
    }
}
