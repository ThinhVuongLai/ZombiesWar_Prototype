namespace App.Level
{
    public readonly struct LevelLoadedMessage
    {
        public readonly int LevelId;

        public LevelLoadedMessage(int levelId)
        {
            LevelId = levelId;
        }
    }

    public readonly struct LevelStartedMessage
    {
        public readonly int LevelId;

        public LevelStartedMessage(int levelId)
        {
            LevelId = levelId;
        }
    }

    public readonly struct LevelCompletedMessage
    {
        public readonly int LevelId;
        public readonly bool IsPlayerWin;

        public LevelCompletedMessage(int levelId, bool isPlayerWin)
        {
            LevelId = levelId;
            IsPlayerWin = isPlayerWin;
        }
    }

    public readonly struct LevelClearedMessage
    {
        public readonly int LevelId;

        public LevelClearedMessage(int levelId)
        {
            LevelId = levelId;
        }
    }
}
