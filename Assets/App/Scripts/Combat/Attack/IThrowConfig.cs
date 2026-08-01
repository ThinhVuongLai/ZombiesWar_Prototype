using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Combat.Attack
{
    public interface IThrowConfig
    {
        float ThrowAngle { get; }
        float MinimumThrowForce { get; }
        float MaximumThrowForce { get; }
        float GravityScale { get; }
        float ObjectLifespan { get; }
        float ActionRadius { get; }
        ThrowActionType ActionType { get; }
        GameObject ObjectPrefab { get; }
    }
}
