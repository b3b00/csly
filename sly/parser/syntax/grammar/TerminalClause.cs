using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
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

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(ExpectedToken);
            if (Discarded) b.Append("[d]");
            return b.ToString();
        }
        
        public string Dump()
        {
            return ExpectedToken.ToString();
        }
    }
}