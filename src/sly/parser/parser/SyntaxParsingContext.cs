using System.Collections.Generic;
using System.Diagnostics;
using sly.parser.syntax.grammar;

namespace sly.parser
{
   
    public class SyntaxParsingContext<IN> where IN : struct
    {
        private readonly Dictionary<string, SyntaxParseResult<IN>> _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN>>();

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