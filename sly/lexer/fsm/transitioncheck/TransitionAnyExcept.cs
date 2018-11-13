using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAnyExcept : AbstractTransitionCheck
    {
        private readonly List<char> TokenExceptions;

        public TransitionAnyExcept(params char[] tokens)
        {
            TokenExceptions = new List<char>();
            TokenExceptions.AddRange(tokens);
        }

        public TransitionAnyExcept(TransitionPrecondition precondition, params char[] tokens)
        {
            TokenExceptions = new List<char>();
            TokenExceptions.AddRange(tokens);
            Precondition = precondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var t = "";
            if (Precondition != null) t = "[|] ";
            t += $"^({TokenExceptions})";
            return t;
        }

        public override bool Match(char input)
        {
            return !TokenExceptions.Contains(input);
        }
    }
}