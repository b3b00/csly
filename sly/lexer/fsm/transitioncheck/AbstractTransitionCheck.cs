using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sly.lexer.fsm.transitioncheck
{

    [ExcludeFromCodeCoverage]
    public static class CharExt {

        public static string ToEscaped(this char c) {
            List<char> ToEscape = new List<char>() {'"','\\'};
            if (ToEscape.Contains(c)) {
                return "\\"+c;
            }
            return c+"";
        }
    }
    public abstract class AbstractTransitionCheck
    {
        public TransitionPrecondition Precondition { get; set; }
        public abstract bool Match(char input);

        public bool Check(char input, string value)
        {
            var match = true;
            if (Precondition != null) match = Precondition(value);
            if (match) match = Match(input);
            return match;
        }

        public abstract string ToGraphViz();
    }
}