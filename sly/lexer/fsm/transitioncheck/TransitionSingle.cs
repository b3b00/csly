using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionSingle : AbstractTransitionCheck
    {
        private char TransitionToken;

        public TransitionSingle(char token)
        {
            TransitionToken = token;
        }



        public TransitionSingle(char token, TransitionPrecondition precondition)
        {
            TransitionToken = token;
            Precondition = precondition;
        }

        public override string ToString()  {
            string t = "";
            if (Precondition != null) {
                t = "[|] ";
            }
            t+=TransitionToken;
            return t;
        }

        public override bool Match(char input)
        {
            return input.Equals(TransitionToken);
        }
    }
}
