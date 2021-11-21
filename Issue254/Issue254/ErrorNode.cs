using System;
using System.Collections.Generic;

namespace BravoLights.Common.Ast
{
    public class ErrorNode : IAstNode
    {
        public string ErrorText { get; set; }

        public IEnumerable<IVariable> Variables {
            get { return Array.Empty<IVariable>(); }
        }

        event EventHandler<ValueChangedEventArgs> IAstNode.ValueChanged
        {
            add { }
            remove { }
        }

        public override string ToString()
        {
            return ErrorText;
        }
    }
}
