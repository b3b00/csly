using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAny : AbstractTransitionCheck
    {

        public TransitionAny(char token)
        {
        }

        public TransitionAny(char token, TransitionPrecondition transitionPrecondition)
        {
            Precondition = transitionPrecondition;
        }

        public override bool Match(char input)
        {
            return true;
        }
    }
}
