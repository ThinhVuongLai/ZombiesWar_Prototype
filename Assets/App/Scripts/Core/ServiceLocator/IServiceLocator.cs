using System;

namespace App.Core.Services
{
    public interface IServiceLocator
    {
        T Resolve<T>();
        object Resolve(Type type);
    }
}
