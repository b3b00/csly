using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionMany : AbstractTransitionCheck
    {
        private readonly char[] _transitionToken;

        public TransitionMany(char[] token)
        {
            _transitionToken = token;
        }


        public TransitionMany(char[] token, TransitionPrecondition precondition)
        {
            _transitionToken = token;
            Precondition = precondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToGraphViz()
        {
            var t = "";
            if (Precondition != null)
            {
                t = "[|] ";
            }
            t += "["+string.Join(",",_transitionToken.Select<char, string>(x => x.ToEscaped()))+"]";
            return $@"[ label=""{t}"" ]";
        }

        public override bool Match(char input)
        {
            return _transitionToken.Contains<char>(input);
        }
    }
}