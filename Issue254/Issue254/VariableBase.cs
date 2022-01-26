using System;
using System.Collections.Generic;

namespace BravoLights.Common.Ast
{
    /// <summary>
    /// Base class for a variable.
    /// </summary>
    public abstract class VariableBase : IVariable
    {
        public string ErrorText { get { return null; } }

        public IEnumerable<IVariable> Variables
        {
            get { yield return this; }
        }

        protected abstract IConnection Connection { get; }
        public abstract string Identifier { get; }

        private readonly Dictionary<EventHandler<ValueChangedEventArgs>, EventHandler<ValueChangedEventArgs>> handlerMappings
            = new();

        public event EventHandler<ValueChangedEventArgs> ValueChanged
        {
            add
            {
                // For this incoming subscription we're just going to subscribe to the
                // underlying connection but with a callback that changes the sender
                // to be this variable.
                void mappedDelegate(object sender, ValueChangedEventArgs e)
                {
                    value(this, e);
                }

                handlerMappings[value] = mappedDelegate;
                Connection.AddListener(this, mappedDelegate);
            }
            remove
            {
                var mappedDelegate = handlerMappings[value];
                handlerMappings.Remove(value);
                Connection.RemoveListener(this, mappedDelegate);
            }
        }
        
        public bool Equals(IVariable other)
        {
            return Identifier.Equals(other.Identifier);
        }
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}
