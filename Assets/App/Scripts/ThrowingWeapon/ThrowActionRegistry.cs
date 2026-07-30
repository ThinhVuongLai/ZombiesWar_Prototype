using System.Collections.Generic;

namespace ZombiesWar.ThrowingWeapon
{
    public class ThrowActionRegistry
    {
        readonly Dictionary<ThrowActionType, IThrowAction> _actions = new();

        public ThrowActionRegistry()
        {
            _actions[ThrowActionType.Explosion] = new ExplosionThrowAction();
        }

        public IThrowAction GetAction(ThrowActionType type)
        {
            _actions.TryGetValue(type, out var action);
            return action;
        }
    }
}
