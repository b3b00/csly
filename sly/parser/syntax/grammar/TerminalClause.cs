using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    public class TerminalClause<T> : IClause<T> where T : struct
    {
        public TerminalClause(LeadingToken<T> token)
        {
            ExpectedToken = token;
        }

        public TerminalClause(LeadingToken<T> token, bool discard) : this(token)
        {
            Discarded = discard;
        }
        
        public TerminalClause(string explicitToken, bool discard) : this(new LeadingToken<T>(default(T),explicitToken))
        {
            ExplicitToken = explicitToken;
            Discarded = discard;
        }
        
        public TerminalClause(string explicitToken) : this(explicitToken,false)
        {
        }

        public LeadingToken<T> ExpectedToken { get; set; }

        public string ExplicitToken { get; set; }

        public bool IsExplicitToken => !string.IsNullOrEmpty(ExplicitToken);
        
        public bool Discarded { get; set; }

        public virtual bool MayBeEmpty()
        {
            return false;
        }

        public virtual bool Check(Token<T> nextToken)
        {
            return ExpectedToken.Match(nextToken);
            if (IsExplicitToken)
            {
                return nextToken.Value.Equals(ExplicitToken);
            }
            return nextToken.TokenID.Equals(ExpectedToken.TokenId);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var b = new StringBuilder();
            if (IsExplicitToken)
            {
                b.Append($"'{ExplicitToken}'");
            }
            else
            {
                b.Append(ExpectedToken);
            }

            if (Discarded) b.Append("[d]");
            b.Append("(T)");
            return b.ToString();
        }
        
        public virtual string Dump()
        {

            if (IsExplicitToken)
            {
                return $"'{ExplicitToken}'(T)";
            }
            return $"{ExpectedToken}(T)";
        }
    }


    public enum IndentationType
    {
        Indent,
        UnIndent
    }
    
    public class IndentTerminalClause<T> : TerminalClause<T> where T : struct
    {
        private IndentationType ExpectedIndentation;
        
        public IndentTerminalClause(IndentationType expectedIndentation, bool discard) : base(default(LeadingToken<T>))
        {
            ExpectedIndentation = expectedIndentation;
            Discarded = discard;
        }
    
        public override  bool MayBeEmpty()
        {
            return false;
        }
    
        public override bool Check(Token<T> nextToken)
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