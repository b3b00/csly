using System.Collections.Generic;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    public class SyntaxParsingContext<IN> where IN : struct
    {
        private Dictionary<(IClause<IN> clause, int position), SyntaxParseResult<IN>> _memoizedNonTerminalResults;

        public SyntaxParsingContext()
        {
            _memoizedNonTerminalResults = new Dictionary<(IClause<IN> clause, int position), SyntaxParseResult<IN>>();
        }

        public void Memoize(IClause<IN> clause, int position, SyntaxParseResult<IN> result)
        {
            _memoizedNonTerminalResults[(clause,position)] = result;
        }

        public bool TryGetParseResult(IClause<IN> clause, int position, out SyntaxParseResult<IN> result)
        {
            return _memoizedNonTerminalResults.TryGetValue((clause, position), out result);
        }
    }
}