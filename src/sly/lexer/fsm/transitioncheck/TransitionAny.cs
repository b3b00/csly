using System.Diagnostics.CodeAnalysis;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionAny : AbstractTransitionCheck
    {
        public TransitionAny()
        {
        }

        public TransitionAny(TransitionPrecondition transitionPrecondition)
        {
            Precondition = transitionPrecondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToGraphViz()
        {
            var label = Precondition != null ? "[|]*" : "*";
            return  $@"[ label=""{label}"" ]";
        }

        public override bool Match(char input)
        {
            return true;
        }
    }
}