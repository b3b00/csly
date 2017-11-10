using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
   public class TransitionSingle : ITransitionCheck
    {
        private char TransitionToken;

        public TransitionSingle(char token) {
            TransitionToken = token;
        }

        public bool Match(char input)
        {
            return input.Equals(TransitionToken);
        }
    }
}
