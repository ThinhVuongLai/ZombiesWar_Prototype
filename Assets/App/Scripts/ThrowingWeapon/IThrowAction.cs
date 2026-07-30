using UnityEngine;

namespace ZombiesWar.ThrowingWeapon
{
    public interface IThrowAction
    {
        ThrowActionType ActionType { get; }
        void Execute(Vector3 position, float radius, float damage);
    }
}
