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
        var entityManager = world.EntityManager;

        var playerTargetEntity = entityManager.CreateEntity(typeof(PlayerTargetECSData));
        entityManager.SetComponentData(playerTargetEntity, new PlayerTargetECSData
        {
            Position = Unity.Mathematics.float3.zero,
            IsAlive = true,
        });

        var playerHealthEntity = entityManager.CreateEntity(typeof(PlayerHealth), typeof(LocalTransform));
        entityManager.SetComponentData(playerHealthEntity, new PlayerHealth
        {
            Value = 100f,
            MaxValue = 100f,
        });
        entityManager.SetComponentData(playerHealthEntity, LocalTransform.FromPosition(Unity.Mathematics.float3.zero));

        var weaponTargetEntity = entityManager.CreateEntity(typeof(PlayerWeaponTargetData));
        entityManager.SetComponentData(weaponTargetEntity, new PlayerWeaponTargetData
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
            simGroup.AddSystemToUpdateList(enemyGroup);

            simGroup.AddSystemToUpdateList(world.CreateSystem<PlayerWeaponDetectionSystem>());

            simGroup.AddSystemToUpdateList(world.CreateSystem<BulletSystem>());
        }
    }

    public static void Shutdown()
    {
        if (!_initialized) return;
        _initialized = false;

        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        DestroySingletonEntity<PlayerTargetECSData>(entityManager);
        DestroySingletonEntity<PlayerHealth>(entityManager);
        DestroySingletonEntity<PlayerWeaponTargetData>(entityManager);

        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        if (simGroup != null)
        {
            RemoveSystemIfExists<BulletSystem>(world, simGroup);
            RemoveSystemIfExists<PlayerWeaponDetectionSystem>(world, simGroup);

            var enemyGroup = world.GetExistingSystemManaged<EnemySystemGroup>();
            if (enemyGroup != null)
            {
                RemoveSystemIfExists<EnemyDetectionSystem>(world, enemyGroup);
                simGroup.RemoveSystemFromUpdateList(enemyGroup);
            }
        }
    }

    static void DestroySingletonEntity<T>(EntityManager entityManager) where T : unmanaged, IComponentData
    {
        var query = entityManager.CreateEntityQuery(typeof(T));
        if (query.CalculateEntityCount() > 0)
        {
            entityManager.DestroyEntity(query.GetSingletonEntity());
        }
    }

    static void RemoveSystemIfExists<T>(World world, ComponentSystemGroup group) where T : unmanaged, ISystem
    {
        var system = world.GetExistingSystem<T>();
        if (system != SystemHandle.Null)
        {
            group.RemoveSystemFromUpdateList(system);
        }
    }
}
