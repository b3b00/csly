using System;
using BravoLights.Common;

namespace BravoLights.Common
{
    public interface IConnection
    {
        void AddListener(IVariable variable, EventHandler<ValueChangedEventArgs> handler);
        void RemoveListener(IVariable variable, EventHandler<ValueChangedEventArgs> handler);
    }
}
