using VContainer;
using VContainer.Unity;
using App.Core.Services;

namespace App.Core.Installers
{
    internal sealed class CoreInitializer : IInitializable
    {
        private readonly IObjectResolver _resolver;

        public CoreInitializer(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Initialize()
        {
            ServiceLocator.Initialize(_resolver);
        }
    }
}
