using R3;
using UnityEngine;

namespace App.Player.Combat
{
    public class PlayerCombatModel
    {
        public ReactiveProperty<bool> HasTarget { get; } = new(false);
        public ReactiveProperty<Vector3> TargetDirection { get; } = new(Vector3.forward);
        public ReactiveProperty<float> AttackRadius { get; } = new(3f);
        public ReactiveProperty<float> AttackCooldown { get; } = new(0.5f);
    }
}
