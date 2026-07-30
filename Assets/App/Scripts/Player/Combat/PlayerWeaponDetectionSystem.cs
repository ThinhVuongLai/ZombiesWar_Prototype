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
        float3 playerPos = SystemAPI.GetSingleton<PlayerTargetECSData>().Position;
        var weaponDataRW = SystemAPI.GetSingletonRW<PlayerWeaponTargetData>();

        float attackRadiusSq = weaponDataRW.ValueRO.AttackRadius * weaponDataRW.ValueRO.AttackRadius;
        Entity currentTarget = weaponDataRW.ValueRO.CurrentTargetEntity;

        if (TryKeepCurrentTarget(state.EntityManager, currentTarget, playerPos, attackRadiusSq, ref weaponDataRW))
            return;

        FindNewTarget(_enemyQuery, playerPos, attackRadiusSq, ref weaponDataRW);
    }

    static bool TryKeepCurrentTarget(in EntityManager em, Entity currentTarget, float3 playerPos,
        float attackRadiusSq, ref RefRW<PlayerWeaponTargetData> weaponDataRW)
    {
        if (currentTarget == Entity.Null)
            return false;

        if (!em.Exists(currentTarget))
            return false;

        if (!em.HasComponent<LocalTransform>(currentTarget) || !em.HasComponent<EnemyHealth>(currentTarget))
            return false;

        var targetTransform = em.GetComponentData<LocalTransform>(currentTarget);
        var targetHealth = em.GetComponentData<EnemyHealth>(currentTarget);

        float sqrDist = math.distancesq(targetTransform.Position, playerPos);

        if (targetHealth.Value > 0f && sqrDist <= attackRadiusSq)
        {
            float3 dir = targetTransform.Position - playerPos;
            weaponDataRW.ValueRW.TargetDirection = math.normalize(dir);
            return true;
        }

        return false;
    }

    static void FindNewTarget(in EntityQuery query, float3 playerPos, float attackRadiusSq,
        ref RefRW<PlayerWeaponTargetData> weaponDataRW)
    {
        using var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        using var healths = query.ToComponentDataArray<EnemyHealth>(Allocator.Temp);
        using var entities = query.ToEntityArray(Allocator.Temp);

        float closestDistSq = float.MaxValue;
        Entity closestEntity = Entity.Null;
        float3 closestPos = float3.zero;

        for (int i = 0; i < entities.Length; i++)
        {
            if (healths[i].Value <= 0f)
                continue;

            float sqrDist = math.distancesq(transforms[i].Position, playerPos);
            if (sqrDist <= attackRadiusSq && sqrDist < closestDistSq)
            {
                closestDistSq = sqrDist;
                closestEntity = entities[i];
                closestPos = transforms[i].Position;
            }
        }

        if (closestEntity != Entity.Null)
        {
            weaponDataRW.ValueRW.CurrentTargetEntity = closestEntity;
            weaponDataRW.ValueRW.TargetPosition = closestPos;
            weaponDataRW.ValueRW.TargetDirection = math.normalize(closestPos - playerPos);
            weaponDataRW.ValueRW.HasTarget = true;
        }
        else
        {
            weaponDataRW.ValueRW.CurrentTargetEntity = Entity.Null;
            weaponDataRW.ValueRW.TargetPosition = float3.zero;
            weaponDataRW.ValueRW.TargetDirection = float3.zero;
            weaponDataRW.ValueRW.HasTarget = false;
        }
    }
}
