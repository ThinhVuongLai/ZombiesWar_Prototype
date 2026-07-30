using R3;

namespace App.Enemy
{
    public enum EnemyStateType
    {
        Idle,
        Move,
        Attack,
        Die
    }

    public class EnemyModel
    {
        public ReactiveProperty<float> MoveSpeed { get; } = new(3.5f);
        public ReactiveProperty<float> Health { get; } = new(100f);
        public ReactiveProperty<float> MaxHealth { get; } = new(100f);
        public ReactiveProperty<float> AttackDamage { get; } = new(10f);
        public ReactiveProperty<float> AttackCooldown { get; } = new(1.5f);
        public ReactiveProperty<float> DetectionRange { get; } = new(12f);
        public ReactiveProperty<EnemyStateType> CurrentState { get; } = new(EnemyStateType.Idle);
    }
}
