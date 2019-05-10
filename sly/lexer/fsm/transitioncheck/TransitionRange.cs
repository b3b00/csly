using System.Diagnostics.CodeAnalysis;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionRange : AbstractTransitionCheck
    {
        private readonly char RangeEnd;
        private readonly char RangeStart;

        public TransitionRange(char start, char end)
        {
            RangeStart = start;
            RangeEnd = end;
        }


        public TransitionRange(char start, char end, TransitionPrecondition precondition)
        {
            RangeStart = start;
            RangeEnd = end;
            Precondition = precondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToGraphViz()
        {
            var t = "";
            if (Precondition != null) t = "[|] ";
            t += $"[{RangeStart.ToEscaped()}-{RangeEnd.ToEscaped()}]";
            return $@"[ label=""{t}"" ]";
        }


        public override bool Match(char input)
        {
            return input.CompareTo(RangeStart) >= 0 && input.CompareTo(RangeEnd) <= 0;
        }
    }
}