using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionRange<I> : ITransitionCheck<I> where I: struct, IComparable
    {
        I RangeStart;

        I RangeEnd;

        public TransitionRange(I start, I end)
        {
            RangeStart = start;
            RangeEnd = end;
        }

        public bool Match(I input)
        {
            throw new NotImplementedException();
        }
    }
}
