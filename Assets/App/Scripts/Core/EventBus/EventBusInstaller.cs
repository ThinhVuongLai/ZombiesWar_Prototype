using VContainer;
using VContainer.Unity;

namespace App.Core.EventBus
{
    public sealed class EventBusInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
        }
    }
}
