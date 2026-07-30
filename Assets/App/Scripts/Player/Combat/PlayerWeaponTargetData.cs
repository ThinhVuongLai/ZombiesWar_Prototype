using Unity.Entities;
using Unity.Mathematics;

public struct PlayerWeaponTargetData : IComponentData
{
    public float AttackRadius;
    public Entity CurrentTargetEntity;
    public float3 TargetPosition;
    public float3 TargetDirection;
    public bool HasTarget;
}
