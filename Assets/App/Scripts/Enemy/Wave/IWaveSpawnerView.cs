using UnityEngine;

namespace App.Enemy.Wave
{
    public interface IWaveSpawnerView
    {
        Vector3 WorldCenter { get; }
        Vector2 ScreenExtents { get; }
        Camera GameCamera { get; }
    }
}
