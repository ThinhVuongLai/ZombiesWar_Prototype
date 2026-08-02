using System;
using System.Collections.Concurrent;
using VContainer;

namespace App.Core.Services
{
    public static class ServiceLocator
    {
        static readonly ConcurrentDictionary<Type, object> _manualRegistry = new();
        static IObjectResolver _resolver;
        static volatile bool _initialized;
        static readonly object _lock = new();

        public static bool Initialized => _initialized;

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

        public static void Register<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            _manualRegistry[typeof(T)] = instance;
        }

        public static void Unregister<T>()
        {
            _manualRegistry.TryRemove(typeof(T), out _);
        }

        public static T Resolve<T>()
        {
            if (_manualRegistry.TryGetValue(typeof(T), out var manualInstance))
                return (T)manualInstance;

            ThrowIfNotInitialized();
            return _resolver.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (_manualRegistry.TryGetValue(type, out var manualInstance))
                return manualInstance;

            ThrowIfNotInitialized();
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
                _manualRegistry.Clear();
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
