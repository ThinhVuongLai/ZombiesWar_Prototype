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
        Action OnDestroyed { get; set; }
        Action<float> TakeExternalDamage { get; set; }
        IHealthBarView CreateHealthBar();
    }
}
