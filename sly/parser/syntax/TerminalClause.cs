using System.Text;

namespace sly.parser.syntax
{
    public class TerminalClause<T> : IClause<T>
    {
        public TerminalClause(T token)
        {
            ExpectedToken = token;
        }

        public TerminalClause(T token, bool discard) : this(token)
        {
            Discarded = discard;
        }

        public T ExpectedToken { get; set; }

        public bool Discarded { get; set; }

        public bool MayBeEmpty()
        {
            return false;
        }

        public bool Check(T nextToken)
        {
            return nextToken.Equals(ExpectedToken);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(ExpectedToken);
            if (Discarded) b.Append("[d]");
            return b.ToString();
        }
    }
}