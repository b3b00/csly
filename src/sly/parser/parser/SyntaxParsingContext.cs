using System;
using System.Collections.Generic;
using System.Diagnostics;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    internal class MemoizerKey<T> : IEquatable<MemoizerKey<T>>, IComparable<MemoizerKey<T>> where T : struct
    {
        public IClause<T> Clause { get; set; }
        
        public int Position { get; set; }

        public MemoizerKey(IClause<T> clause, int position)
        {
            Clause = clause;
            Position = position;
        }

        public bool Equals(MemoizerKey<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Clause, other.Clause) && Position == other.Position;
        }

        public int CompareTo(MemoizerKey<T> other)
        {
            if (other != null)
            {
                return Equals(other) ? 0 : 1;
            }

            return 1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemoizerKey<T>)obj);
        }

    } 
    public class SyntaxParsingContext<IN> where IN : struct
    {
        private Dictionary<string, SyntaxParseResult<IN>> _memoizedNonTerminalResults;

        public SyntaxParsingContext()
        {
            _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN>>();
        }

        private string GetKey(IClause<IN> clause, int position)
        {
            return $"{clause.Dump()} -- @{position}";
        }
        
        public void Memoize(IClause<IN> clause, int position, SyntaxParseResult<IN> result)
        {
            _memoizedNonTerminalResults[GetKey(clause,position)] = result;
        }

        public bool TryGetParseResult(IClause<IN> clause, int position, out SyntaxParseResult<IN> result)
        {
            bool found = _memoizedNonTerminalResults.TryGetValue(GetKey(clause, position), out result);
            if (!found)
            {
                Debug.WriteLine($"NOT FOUND ! {clause} - {position}");
            }
            return found;
        }
    }
}