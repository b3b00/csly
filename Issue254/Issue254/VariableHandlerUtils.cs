using System;
using BravoLights.Common;

namespace BravoLights.Connections
{
    static class VariableHandlerUtils
    {
        /// <summary>
        /// Reports that, whilst we are connected to the server, we haven't yet received a value for this variable.
        /// </summary>
        public static void SendNoValueError(object sender, EventHandler<ValueChangedEventArgs> handler)
        {
            handler(sender, new ValueChangedEventArgs { NewValue = new Exception("No value yet received from simulator") });
        }

        /// <summary>
        /// Reports that a variable doesn't have a value because the simulator isn't connected.
        /// </summary>
        public static void SendNoConnectionError(object sender, EventHandler<ValueChangedEventArgs> handler)
        {
            handler(sender, new ValueChangedEventArgs { NewValue = new Exception("No connection to simulator") });
        }
    }
}
