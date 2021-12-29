using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    public class TerminalClause<IN,OUT> : IClause<IN,OUT>
    {
        public TerminalClause(IN token)
        {
            ExpectedToken = token;
        }

        public TerminalClause(IN token, bool discard) : this(token)
        {
            Discarded = discard;
        }

        public IN ExpectedToken { get; set; }

        public bool Discarded { get; set; }

        public virtual bool MayBeEmpty()
        {
            return false;
        }

        public virtual bool Check(Token<IN> nextToken)
        {
            return nextToken.TokenID.Equals(ExpectedToken);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(ExpectedToken);
            if (Discarded) b.Append("[d]");
            return b.ToString();
        }
        
        public virtual string Dump()
        {
            return ExpectedToken.ToString();
        }
    }


    public enum IndentationType
    {
        Indent,
        UnIndent
    }
    
    public class IndentTerminalClause<IN,OUT> : TerminalClause<IN,OUT>
    {
        private IndentationType ExpectedIndentation;
        
        public IndentTerminalClause(IndentationType expectedIndentation, bool discard) : base(default(IN))
        {
            ExpectedIndentation = expectedIndentation;
            Discarded = discard;
        }
    
        public override  bool MayBeEmpty()
        {
            return false;
        }
    
        public override bool Check(Token<IN> nextToken)
        {
            return (nextToken.IsIndent && ExpectedIndentation == IndentationType.Indent) ||
                   (nextToken.IsUnIndent && ExpectedIndentation == IndentationType.UnIndent);
        }
    
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(ExpectedIndentation == IndentationType.Indent ? "TAB" : "UNTAB" );
            if (Discarded) b.Append("[d]");
            return b.ToString();
        }
        
        public override string Dump()
        {
            return ToString();
        }
    }
}