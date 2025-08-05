using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public class TransitionMultiRange : AbstractTransitionCheck
    {

        private readonly (char start, char end)[] _ranges;

        public TransitionMultiRange(params (char start, char end)[] ranges)
        {
            this._ranges = ranges;
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
            foreach (var range in _ranges)
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
            while (!match && i < _ranges.Length)
            {
                var range = _ranges[i];
                match = input.CompareTo(range.start) >= 0 && input.CompareTo(range.end) <= 0;
                i++;
            }

            return match;
        }
    }
}