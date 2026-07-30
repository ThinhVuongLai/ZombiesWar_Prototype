using System;
using VContainer;

namespace App.Core.Services
{
    public static class ServiceLocator
    {
        private static IObjectResolver _resolver;
        private static volatile bool _initialized;
        private static readonly object _lock = new();

        public static void Initialize(IObjectResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            lock (_lock)
            {
                if (_initialized)
                    throw new InvalidOperationException(
                        "ServiceLocator is already initialized. Initialize() must be called exactly once.");

                _resolver = resolver;
                _initialized = true;
            }
        }

        public static T Resolve<T>()
        {
            ThrowIfNotInitialized();
            return _resolver.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            ThrowIfNotInitialized();
            if (type == null) throw new ArgumentNullException(nameof(type));
            return _resolver.Resolve(type);
        }

        private static void ThrowIfNotInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "ServiceLocator has not been initialized. " +
                    "Ensure CoreInstaller is installed in the root LifetimeScope.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void Reset()
        {
            lock (_lock)
            {
                _resolver = null;
                _initialized = false;
            }
        }
#endif
    }

    internal sealed class InjectableServiceLocator : IServiceLocator
    {
        public T Resolve<T>() => ServiceLocator.Resolve<T>();
        public object Resolve(Type type) => ServiceLocator.Resolve(type);
    }
}
