using System;
using System.Diagnostics;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    [DebuggerDisplay("Terminal {(ExpectedToken.ToString())}{Discarded ? \"[d]\" : \"\"}")]
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

        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return ToString();
        }


        public bool Equals(IClause<T> clause)
        {
            if (clause is TerminalClause<T> other)
            {
                return Equals(other);
            }

            return false;
        }
        protected bool Equals(TerminalClause<T> other)
        {
            if (IsExplicitToken)
            {
                return other.ExplicitToken == ExplicitToken;
            }
            
            return ExpectedToken.Equals(other.ExpectedToken);
        }

    }


    public enum IndentationType
    {
        Indent,
        UnIndent
    }
    
    public sealed class IndentTerminalClause<T> : TerminalClause<T> where T : struct
    {
        private IndentationType ExpectedIndentation;
        
        public IndentTerminalClause(IndentationType expectedIndentation, bool discard) : base(new LeadingToken<T>(default(T)))
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
        
    }
}