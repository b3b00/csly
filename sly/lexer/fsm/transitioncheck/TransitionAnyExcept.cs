using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAnyExcept<I> : ITransitionCheck<I> where I : struct, IComparable
    {
        I TokenException;

        public TransitionAnyExcept(I token)
        {
            TokenException = token;
        }

        public bool Match(I input)
        {
            return !input.Equals(TokenException);
        }
    }
}
