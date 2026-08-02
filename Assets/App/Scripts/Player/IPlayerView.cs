using System;
using App.HealthBar;
using UnityEngine;
using ZombiesWar.Weapon;

namespace App.Player
{
    public interface IPlayerView
    {
        void Move(Vector3 motion);
        void FaceDirection(Vector3 direction);
        void PlayMoveAnimation(string animationName, int layerIndex);
        void PlayAttackAnimation(string animationName, int layerIndex);
        bool IsGrounded { get; }
        Transform Transform { get; }
        IHealthBarView CreateHealthBar();
        void PlayDamageFlash(Color flashColor, float duration);
        void SetWeaponModel(WeaponBase weaponConfig);
    }
}