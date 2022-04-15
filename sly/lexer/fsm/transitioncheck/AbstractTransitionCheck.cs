using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;

namespace sly.lexer.fsm.transitioncheck
{

    [ExcludeFromCodeCoverage]
    public static class CharExt
    {

        public static string ToEscaped(this char c)
        {
            List<char> ToEscape = new List<char> { '"', '\\' };
            if (ToEscape.Contains(c))
            {
                return "\\" + c;
            }
            return c + "";
        }
    }
    public abstract class AbstractTransitionCheck
    {
        public TransitionPrecondition Precondition { get; set; }
        public abstract bool Match(char input);

        public bool Check(char input, ReadOnlyMemory<char> value)
        {
            var match = true;
            if (Precondition != null) match = Precondition(value);
            if (match) match = Match(input);
            return match;
        }

        [ExcludeFromCodeCoverage]
        public abstract string ToGraphViz();
    }
}