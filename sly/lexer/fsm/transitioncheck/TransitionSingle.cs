using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
   public class TransitionSingle<I> : ITransitionCheck<I> where I : struct, IComparable
    {
        I TransitionToken;
        private I token;

        public TransitionSingle(I token) {
            TransitionToken = token;
        }

        public bool Match(I input)
        {
            return input.Equals(TransitionToken);
        }
    }
}
