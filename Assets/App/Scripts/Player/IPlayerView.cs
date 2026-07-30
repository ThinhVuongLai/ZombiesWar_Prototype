using System;
using UnityEngine;

namespace App.Player
{
    public interface IPlayerView
    {
        void Move(Vector3 motion);
        void FaceDirection(Vector3 direction);
        bool IsGrounded { get; }
        Transform Transform { get; }
    }
}