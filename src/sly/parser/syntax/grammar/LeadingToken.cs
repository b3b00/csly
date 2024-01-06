using System;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    public sealed class LeadingToken<IN> : IEquatable<LeadingToken<IN>> where IN:struct 
    {
        public IN TokenId { get; set; }
        
        public string ExplicitToken { get; set; }
        
        public bool IsExplicitToken { get; set; }

        public LeadingToken(IN tokenId)
        {
            TokenId = tokenId;
            IsExplicitToken = false;
        }
        
        public LeadingToken(IN tokenId, string explicitToken)
        {
            TokenId = tokenId;
            ExplicitToken = explicitToken;
            IsExplicitToken = true;
        }

        public bool Match(Token<IN> token)
        {
            if (IsExplicitToken)
            {
                return ExplicitToken == token.Value;
            }
            else
            {
                return TokenId.Equals(token.TokenID);
            }
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsExplicitToken)
            {
                return $"'{ExplicitToken}'";
            }
            else
            {
                return TokenId.ToString();
            }
        }

        public bool Equals(LeadingToken<IN> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (IsExplicitToken)
            {
                return other.IsExplicitToken && ExplicitToken == other.ExplicitToken;
            }
            return TokenId.Equals(other.TokenId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LeadingToken<IN>)obj);
        }

        public override int GetHashCode()
        {
            return IsExplicitToken ? this.ExplicitToken.GetHashCode() : TokenId.GetHashCode();
        }
    }
}