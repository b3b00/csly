using System;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax.grammar
{
    public sealed class LeadingToken<IN> : IEquatable<LeadingToken<IN>> where IN:struct, Enum
    {
        public IN TokenId { get; set; }
        
        public string ExplicitToken { get; set; }
        
        public bool IsExplicitToken { get; set; }
        
        public bool IsIndent { get; set; }
        
        public bool IsUnindent { get; set; }
        
        // Optimization: cache the hash code for faster comparisons
        private int? _cachedHashCode;

        public LeadingToken(IN tokenId)
        {
            TokenId = tokenId;
            IsExplicitToken = false;
            _cachedHashCode = null;
        }

        public LeadingToken(bool isIndent, bool isUnindent)
        {
            IsUnindent = isUnindent;
            IsIndent = isIndent;
            _cachedHashCode = null;
        }
        
        public LeadingToken(IN tokenId, string explicitToken)
        {
            TokenId = tokenId;
            ExplicitToken = explicitToken;
            IsExplicitToken = true;
            _cachedHashCode = null;
        }

        public bool Match(Token<IN> token)
        {
            if (IsExplicitToken)
            {
                return ExplicitToken == token.Value;
            }
            if (IsIndent)
            {
                return token.IsIndent;
            }
            if (IsUnindent)
            {
                return token.IsUnIndent;
            }

            return TokenId.Equals(token.TokenID);
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
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = IsExplicitToken ? ExplicitToken.GetHashCode() : TokenId.GetHashCode();
            }
            return _cachedHashCode.Value;
        }
    }
}