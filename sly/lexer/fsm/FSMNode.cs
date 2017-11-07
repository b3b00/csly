using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm
{
    public class FSMNode<N>
    {
        internal N Value { get; set; }

        internal int Id { get; set; } = 0;

        internal bool IsEnd { get; set; } = false;

        internal bool IsStart { get; set; } = false;

        internal FSMNode(N value)
        {
            Value = value;
        }
        
    }
}
