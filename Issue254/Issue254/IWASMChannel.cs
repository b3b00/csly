using System;

namespace BravoLights.Connections
{
    public interface IWASMChannel
    {
        void ClearSubscriptions();
        void Subscribe(short id);
        void Unsubscribe(short id);

        SimState SimState { get; }
        event EventHandler<SimStateEventArgs> OnSimStateChanged;
    }
}
