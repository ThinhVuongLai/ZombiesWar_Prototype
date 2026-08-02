using VContainer;
using VContainer.Unity;

namespace App.Player
{
    public sealed class PlayerInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IPlayerInputProvider, JoystickPlayerInputAdapter>(Lifetime.Singleton);
        }
    }
}