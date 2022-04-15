using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionMultiRange : AbstractTransitionCheck
    {
//        private readonly char RangeEnd;
//        private readonly char RangeStart;

        private (char start, char end)[] ranges;

        public TransitionMultiRange(params (char start, char end)[] ranges)
        {
            this.ranges = ranges;
        }


        public TransitionMultiRange( TransitionPrecondition precondition, params (char start, char end)[] ranges) : this(ranges)
        {
            Precondition = precondition;
        }

        [ExcludeFromCodeCoverage]
        public override string ToGraphViz()
        {
            StringBuilder builder = new StringBuilder();

            if (Precondition != null)
            {
                builder.Append("[|] ");
            }

            builder.Append("[");
            foreach (var range in ranges)
            {
                builder
                    .Append(range.start)
                    .Append("-")
                    .Append(range.end)
                    .Append(",");
            }
            builder.Append("]");
            
            return $@"[ label=""{builder.ToString()}"" ]";
        }


        public override bool Match(char input)
        {
            bool match = false;
            int i = 0;
            while (!match && i < ranges.Length)
            {
                var range = ranges[i];
                match = match ||  input.CompareTo(range.start) >= 0 && input.CompareTo(range.end) <= 0;
                i++;
            }

            return match;
        }
    }
}