using System.Collections.Generic;
using System.Diagnostics;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    
    public class SyntaxParsingContext<IN,OUT> where IN : struct
    {
        private readonly Dictionary<string, SyntaxParseResult<IN>> _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN>>();

        private readonly bool _useMemoization = false;
        public SyntaxParsingContext(bool useMemoization)
        {
            _useMemoization = useMemoization;
        }

        private string GetKey(IClause<IN,OUT> clause, int position)
        {
            return $"{clause.Dump()} -- @{position}";
        }
        
        public void Memoize(IClause<IN,OUT> clause, int position, SyntaxParseResult<IN> result)
        {
            if (_useMemoization)
            {
                _memoizedNonTerminalResults[GetKey(clause, position)] = result;
            }
        }

        public bool TryGetParseResult(IClause<IN,OUT> clause, int position, out SyntaxParseResult<IN> result)
        {
            if (!_useMemoization)
            {
                result = null;
                return false;
            }
            bool found = _memoizedNonTerminalResults.TryGetValue(GetKey(clause, position), out result);
            return found;
        }
    }
}