using System;
using System.Collections.Concurrent;
using R3;

namespace App.Core.EventBus
{
    internal sealed class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, object> _subjects = new();
        private readonly object _lock = new();
        private volatile bool _disposed;

        public void Publish<TMessage>(TMessage message)
        {
            if (_disposed) return;

            if (_subjects.TryGetValue(typeof(TMessage), out var boxed))
            {
                ((Subject<TMessage>)boxed).OnNext(message);
            }
        }

        public Observable<TMessage> On<TMessage>()
        {
            if (_disposed)
                return Observable.Empty<TMessage>();

            return (Subject<TMessage>)_subjects.GetOrAdd(typeof(TMessage), _ =>
            {
                lock (_lock)
                {
                    if (_subjects.TryGetValue(typeof(TMessage), out var existing))
                        return existing;

                    return new Subject<TMessage>();
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var entry in _subjects)
            {
                (entry.Value as IDisposable)?.Dispose();
            }

            _subjects.Clear();
        }
    }
}
