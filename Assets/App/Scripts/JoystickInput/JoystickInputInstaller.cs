using VContainer;
using VContainer.Unity;

namespace App.JoystickInput
{
    public sealed class JoystickInputInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<JoystickInputModel>(Lifetime.Singleton);
            builder.Register<IJoystickInputManager, JoystickInputManager>(Lifetime.Singleton);
        }
    }
}
