using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionRange : ITransitionCheck
    {
        char RangeStart;

        char RangeEnd;

        public TransitionRange(char start, char end)
        {
            RangeStart = start;
            RangeEnd = end;
        }

        public bool Match(char input)
        {
            return input.CompareTo(RangeStart) >= 0  && input.CompareTo(RangeEnd) <= 0; 
        }
    }
}
