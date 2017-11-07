using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAnyExcept : ITransitionCheck
    {
        char TokenException;

        public TransitionAnyExcept(char token)
        {
            TokenException = token;
        }

        public bool Match(char input)
        {
            return !input.Equals(TokenException);
        }
    }
}
