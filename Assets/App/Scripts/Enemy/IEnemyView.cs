using System;
using App.HealthBar;
using UnityEngine;

namespace App.Enemy
{
    public interface IEnemyView
    {
        Transform Transform { get; }
        void SetDestination(Vector3 target);
        void StopMovement();
        bool HasPath { get; }
        float RemainingDistance { get; }
        void SetAgentEnabled(bool enabled);
        void PlayAnimation(string animationName, int layerIndex = 0);
        Action OnDestroyed { get; set; }
        Action<float> TakeExternalDamage { get; set; }
        IHealthBarView CreateHealthBar();
    }
}
