using App.Enemy.Attack;
using VContainer;
using VContainer.Unity;

namespace App.Enemy
{
    public sealed class EnemyInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<AttackStrategyRegistry>(Lifetime.Singleton);
        }
    }
}