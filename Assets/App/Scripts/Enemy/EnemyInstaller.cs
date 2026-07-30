using VContainer;
using VContainer.Unity;

namespace App.Enemy
{
    public sealed class EnemyInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            // AttackStrategyRegistry is created in EnemySpawner with ScriptableObject dependencies
        }
    }
}