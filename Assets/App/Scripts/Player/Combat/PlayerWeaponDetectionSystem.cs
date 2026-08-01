using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerWeaponDetectionSystem : ISystem
{
    EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTargetECSData>();
        state.RequireForUpdate<PlayerWeaponTargetData>();

        _enemyQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<EnemyHealth>()
        );
    }

    public void OnUpdate(ref SystemState state)
    {
        float3 playerPosition = SystemAPI.GetSingleton<PlayerTargetECSData>().Position;
        var weaponDataReadWrite = SystemAPI.GetSingletonRW<PlayerWeaponTargetData>();

        float attackRadiusSquared = weaponDataReadWrite.ValueRO.AttackRadius * weaponDataReadWrite.ValueRO.AttackRadius;
        Entity currentTarget = weaponDataReadWrite.ValueRO.CurrentTargetEntity;

        if (TryKeepCurrentTarget(state.EntityManager, currentTarget, playerPosition, attackRadiusSquared, ref weaponDataReadWrite))
            return;

        FindNewTarget(_enemyQuery, playerPosition, attackRadiusSquared, ref weaponDataReadWrite);
    }

    static bool TryKeepCurrentTarget(in EntityManager entityManager, Entity currentTarget, float3 playerPosition,
        float attackRadiusSquared, ref RefRW<PlayerWeaponTargetData> weaponDataReadWrite)
    {
        if (currentTarget == Entity.Null)
            return false;

        if (!entityManager.Exists(currentTarget))
            return false;

        if (!entityManager.HasComponent<LocalTransform>(currentTarget) || !entityManager.HasComponent<EnemyHealth>(currentTarget))
            return false;

        var targetTransform = entityManager.GetComponentData<LocalTransform>(currentTarget);
        var targetHealth = entityManager.GetComponentData<EnemyHealth>(currentTarget);

        float squaredDistance = math.distancesq(targetTransform.Position, playerPosition);

        if (targetHealth.Value > 0f && squaredDistance <= attackRadiusSquared)
        {
            float3 direction = targetTransform.Position - playerPosition;
            weaponDataReadWrite.ValueRW.TargetDirection = math.normalize(direction);
            return true;
        }

        return false;
    }

    static void FindNewTarget(in EntityQuery query, float3 playerPosition, float attackRadiusSquared,
        ref RefRW<PlayerWeaponTargetData> weaponDataReadWrite)
    {
        using var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        using var healths = query.ToComponentDataArray<EnemyHealth>(Allocator.Temp);
        using var entities = query.ToEntityArray(Allocator.Temp);

        float closestDistanceSquared = float.MaxValue;
        Entity closestEntity = Entity.Null;
        float3 closestPosition = float3.zero;

        for (int i = 0; i < entities.Length; i++)
        {
            if (healths[i].Value <= 0f)
                continue;

            float squaredDistance = math.distancesq(transforms[i].Position, playerPosition);
            if (squaredDistance <= attackRadiusSquared && squaredDistance < closestDistanceSquared)
            {
                closestDistanceSquared = squaredDistance;
                closestEntity = entities[i];
                closestPosition = transforms[i].Position;
            }
        }

        if (closestEntity != Entity.Null)
        {
            weaponDataReadWrite.ValueRW.CurrentTargetEntity = closestEntity;
            weaponDataReadWrite.ValueRW.TargetPosition = closestPosition;
            weaponDataReadWrite.ValueRW.TargetDirection = math.normalize(closestPosition - playerPosition);
            weaponDataReadWrite.ValueRW.HasTarget = true;
        }
        else
        {
            weaponDataReadWrite.ValueRW.CurrentTargetEntity = Entity.Null;
            weaponDataReadWrite.ValueRW.TargetPosition = float3.zero;
            weaponDataReadWrite.ValueRW.TargetDirection = float3.zero;
            weaponDataReadWrite.ValueRW.HasTarget = false;
        }
    }
}
