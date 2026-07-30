using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Combat.Attack
{
    public interface IThrowConfig
    {
        float ThrowAngle { get; }
        float MinThrowForce { get; }
        float MaxThrowForce { get; }
        float GravityScale { get; }
        float ObjectLifespan { get; }
        float ActionRadius { get; }
        ThrowActionType ActionType { get; }
        GameObject ObjectPrefab { get; }
    }
}
