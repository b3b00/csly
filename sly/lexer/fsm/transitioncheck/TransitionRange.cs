using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionRange : AbstractTransitionCheck
    {
        char RangeStart;

        char RangeEnd;

        public TransitionRange(char start, char end)
        {
            RangeStart = start;
            RangeEnd = end;
        }



        public TransitionRange(char start, char end, TransitionPrecondition precondition)
        {
            RangeStart = start;
            RangeEnd = end;
            Precondition = precondition;
        }

        public override string ToString()  {
            string t = "";
            if (Precondition != null) {
                t = "[|] ";
            }
            t+=$"[{RangeStart}-{RangeEnd}]";
            return t;
        }


        public override bool Match(char input)
        {
            return input.CompareTo(RangeStart) >= 0 && input.CompareTo(RangeEnd) <= 0;
        }
    }
}
