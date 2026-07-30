using R3;

namespace App.Enemy.Wave
{
    public enum WaveSpawnerState
    {
        Waiting,
        Spawning,
        Active,
        Completed
    }

    public class WaveSpawnerModel
    {
        public ReactiveProperty<int> CurrentWaveIndex { get; } = new(-1);
        public ReactiveProperty<int> EnemiesAlive { get; } = new(0);
        public ReactiveProperty<float> WaveTimer { get; } = new(0f);
        public ReactiveProperty<bool> IsActive { get; } = new(false);
        public ReactiveProperty<WaveSpawnerState> State { get; } = new(WaveSpawnerState.Waiting);
    }
}
