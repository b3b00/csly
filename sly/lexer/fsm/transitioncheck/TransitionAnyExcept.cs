using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAnyExcept : ITransitionCheck
    {
        List<char> TokenExceptions;

        public TransitionAnyExcept(params char[] tokens)
        {
            TokenExceptions = new List<char>();
            TokenExceptions.AddRange(tokens);
        }

        public bool Match(char input)
        {
            return !TokenExceptions.Contains(input);
        }
    }
}
