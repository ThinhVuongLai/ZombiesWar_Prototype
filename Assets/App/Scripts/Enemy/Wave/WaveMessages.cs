namespace App.Enemy.Wave
{
    public readonly struct WaveStartedMessage
    {
        public readonly int WaveIndex;
        public readonly int EnemyCount;

        public WaveStartedMessage(int waveIndex, int enemyCount)
        {
            WaveIndex = waveIndex;
            EnemyCount = enemyCount;
        }
    }

    public readonly struct WaveCompletedMessage
    {
        public readonly int WaveIndex;

        public WaveCompletedMessage(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }

    public readonly struct AllWavesCompletedMessage
    {
    }

    public readonly struct EnemySpawnedMessage
    {
    }

    public readonly struct EnemyDefeatedMessage
    {
    }
}
