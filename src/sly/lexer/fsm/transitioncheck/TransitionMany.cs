using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionMany : AbstractTransitionCheck
    {
        private readonly char[] TransitionToken;

        public TransitionMany(char[] token)
        {
            TransitionToken = token;
        }


        public TransitionMany(char[] token, TransitionPrecondition precondition)
        {
            TransitionToken = token;
            Precondition = precondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToGraphViz()
        {
            var t = "";
            if (Precondition != null) t = "[|] ";
             t += "["+string.Join(",",TransitionToken.Select<char, string>(x => x.ToEscaped()))+"]";
            return $@"[ label=""{t}"" ]";
        }

        public override bool Match(char input)
        {
            return TransitionToken.Contains<char>(input);
        }
    }
}