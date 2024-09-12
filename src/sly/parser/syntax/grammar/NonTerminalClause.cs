using System;
using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class NonTerminalClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }

        public string NonTerminalName { get; set; }

        public bool IsGroup { get; set; } = false;

        public bool MayBeEmpty()
        {
            return false;
        }

        
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return NonTerminalName;
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return $"{NonTerminalName}(NT)";
        }

        public bool Equals(IClause<IN,OUT> clause)
        {
            if (clause is NonTerminalClause<IN,OUT> other)
            {
                return Equals(other);
            }

            return false;
        }

        private bool Equals(NonTerminalClause<IN,OUT> other)
        {
            return NonTerminalName == other.NonTerminalName && IsGroup == other.IsGroup;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NonTerminalClause<IN,OUT>)obj);
        }

        public override int GetHashCode()
        {
            return Dump().GetHashCode();
        }
    }
}