using VContainer;
using VContainer.Unity;
using App.Core.EventBus;
using App.Core.Services;

namespace App.Core.Installers
{
    public sealed class CoreInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IEventBus, EventBus.EventBus>(Lifetime.Singleton);
            builder.Register<IServiceLocator, InjectableServiceLocator>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CoreInitializer>();
        }
    }
}
