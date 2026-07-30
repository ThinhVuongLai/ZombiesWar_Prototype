using App.Player.ECS;
using Unity.Entities;
using Unity.Transforms;
using ZombiesWar.Bullet.ECS;

public static class EnemyECSWorldBootstrap
{
    static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        var world = World.DefaultGameObjectInjectionWorld;
        var em = world.EntityManager;

        var playerTargetEntity = em.CreateEntity(typeof(PlayerTargetECSData));
        em.SetComponentData(playerTargetEntity, new PlayerTargetECSData
        {
            Position = Unity.Mathematics.float3.zero,
            IsAlive = true,
        });

        var playerHealthEntity = em.CreateEntity(typeof(PlayerHealth), typeof(LocalTransform));
        em.SetComponentData(playerHealthEntity, new PlayerHealth
        {
            Value = 100f,
            MaxValue = 100f,
        });
        em.SetComponentData(playerHealthEntity, LocalTransform.FromPosition(Unity.Mathematics.float3.zero));

        var weaponTargetEntity = em.CreateEntity(typeof(PlayerWeaponTargetData));
        em.SetComponentData(weaponTargetEntity, new PlayerWeaponTargetData
        {
            AttackRadius = 3f,
            CurrentTargetEntity = Entity.Null,
            TargetPosition = Unity.Mathematics.float3.zero,
            TargetDirection = Unity.Mathematics.float3.zero,
            HasTarget = false,
        });

        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        if (simGroup != null)
        {
            var enemyGroup = world.CreateSystemManaged<EnemySystemGroup>();
            enemyGroup.AddSystemToUpdateList(world.CreateSystem<EnemyDetectionSystem>());
            enemyGroup.AddSystemToUpdateList(world.CreateSystem<EnemyCombatSystem>());
            simGroup.AddSystemToUpdateList(enemyGroup);

            simGroup.AddSystemToUpdateList(world.CreateSystem<PlayerWeaponDetectionSystem>());

            var bulletGroup = world.CreateSystemManaged<BulletSystemGroup>();
            bulletGroup.AddSystemToUpdateList(world.CreateSystem<BulletMovementSystem>());
            bulletGroup.AddSystemToUpdateList(world.CreateSystem<BulletHitDetectionSystem>());
            bulletGroup.AddSystemToUpdateList(world.CreateSystem<BulletRayCastSystem>());
            bulletGroup.AddSystemToUpdateList(world.CreateSystem<BulletCleanupSystem>());
            simGroup.AddSystemToUpdateList(bulletGroup);
        }
    }
}
