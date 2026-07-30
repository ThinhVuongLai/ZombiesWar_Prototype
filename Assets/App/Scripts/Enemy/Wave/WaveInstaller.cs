using VContainer;
using VContainer.Unity;

namespace App.Enemy.Wave
{
    public sealed class WaveInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<WaveSpawnerManager>(Lifetime.Singleton);
        }
    }
}
