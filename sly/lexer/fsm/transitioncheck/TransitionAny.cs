using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAny : ITransitionCheck
    {

        public TransitionAny(char token)
        {
        }

        public bool Match(char input)
        {
            return true;
        }
    }
}
