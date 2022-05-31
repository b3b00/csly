using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

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
        
        public TerminalClause(string implicitToken, bool discard) : this(default(T))
        {
            ImplicitToken = implicitToken;
            Discarded = discard;
        }

        public T ExpectedToken { get; set; }

        public string ImplicitToken { get; set; }

        public bool IsImplicitToken => !string.IsNullOrEmpty(ImplicitToken);
        
        public bool Discarded { get; set; }

        public virtual bool MayBeEmpty()
        {
            return false;
        }

        public virtual bool Check(Token<T> nextToken)
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
    
    public class IndentTerminalClause<T> : TerminalClause<T>
    {
        private IndentationType ExpectedIndentation;
        
        public IndentTerminalClause(IndentationType expectedIndentation, bool discard) : base(default(T))
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