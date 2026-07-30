using Unity.Entities;
using Unity.Mathematics;

public struct PlayerTargetECSData : IComponentData
{
    public float3 Position;
    public bool IsAlive;
}
