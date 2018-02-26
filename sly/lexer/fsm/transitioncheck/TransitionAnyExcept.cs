using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAnyExcept : AbstractTransitionCheck
    {
        List<char> TokenExceptions;

        public TransitionAnyExcept(params char[] tokens)
        {
            TokenExceptions = new List<char>();
            TokenExceptions.AddRange(tokens);
        }

         public TransitionAnyExcept(TransitionPrecondition precondition,params char[] tokens)
        {
            TokenExceptions = new List<char>();
            TokenExceptions.AddRange(tokens);
            Precondition = precondition;
        }

        public override string ToString()  {
            string t = "";
            if (Precondition != null) {
                t = "[|] ";
            }
            t+=$"^({TokenExceptions.ToString()})";
            return t;
        }

        public override bool Match(char input)
        {
            return !TokenExceptions.Contains(input);
        }
    }
}
