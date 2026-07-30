using System;
using R3;

namespace App.Core.EventBus
{
    public interface IEventBus : IDisposable
    {
        void Publish<TMessage>(TMessage message);
        Observable<TMessage> On<TMessage>();
    }
}
