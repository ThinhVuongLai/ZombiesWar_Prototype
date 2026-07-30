using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace App.Player
{
    public sealed class PlayerInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IPlayerInputProvider, JoystickPlayerInputAdapter>(Lifetime.Singleton);
            builder.Register<IPlayerTargetProvider>(container =>
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                return go != null ? go.GetComponent<PlayerView>() : null;
            }, Lifetime.Singleton);
        }
    }
}