using UnityEngine;

public interface IPlayerTargetProvider
{
    Transform PlayerTransform { get; }
    bool IsAlive { get; }
}
