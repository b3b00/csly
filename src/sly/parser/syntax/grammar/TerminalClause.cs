using System;
using System.Diagnostics;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    [DebuggerDisplay("Terminal {(ExpectedToken.ToString())}{Discarded ? \"[d]\" : \"\"}")]
    public class TerminalClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public TerminalClause(LeadingToken<IN> token)
        {
            ExpectedToken = token;
        }

        public TerminalClause(LeadingToken<IN> token, bool discard) : this(token)
        {
            Discarded = discard;
        }
        
        public TerminalClause(string explicitToken, bool discard) : this(new LeadingToken<IN>(default(IN),explicitToken))
        {
            ExplicitToken = explicitToken;
            Discarded = discard;
        }
        
        public TerminalClause(string explicitToken) : this(explicitToken,false)
        {
        }

        public LeadingToken<IN> ExpectedToken { get; set; }

        public string ExplicitToken { get; set; }

        public bool IsExplicitToken => !string.IsNullOrEmpty(ExplicitToken);
        
        public bool Discarded { get; set; }

        public virtual bool MayBeEmpty()
        {
            return false;
        }

        public virtual bool Check(Token<IN> nextToken)
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


        public bool Equals(IClause<IN,OUT> clause)
        {
            if (clause is TerminalClause<IN,OUT> other)
            {
                return Equals(other);
            }

            return false;
        }
        protected bool Equals(TerminalClause<IN,OUT> other)
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
    
    public sealed class IndentTerminalClause<T,OUT> : TerminalClause<T,OUT> where T : struct
    {
        private IndentationType ExpectedIndentation;
        
        public IndentTerminalClause(IndentationType expectedIndentation, bool discard) : base(new LeadingToken<T>(expectedIndentation == IndentationType.Indent, expectedIndentation == IndentationType.UnIndent ))
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