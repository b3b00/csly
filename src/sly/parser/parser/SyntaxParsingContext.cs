using System;
using System.Collections.Generic;
using sly.parser.parser;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    
    public class SyntaxParsingContext<IN, OUT> where IN : struct, Enum
    {
        private readonly Dictionary<string, SyntaxParseResult<IN, OUT>> _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN, OUT>>();
        
        // Optimization: Pool for error lists to reduce allocations
        private readonly ObjectPool<List<UnexpectedTokenSyntaxError<IN>>> _errorListPool;

        private readonly bool _useMemoization = false;
        
        public SyntaxParsingContext(bool useMemoization)
        {
            _useMemoization = useMemoization;
            _errorListPool = new ObjectPool<List<UnexpectedTokenSyntaxError<IN>>>(
                () => new List<UnexpectedTokenSyntaxError<IN>>(),
                list => list.Clear(),
                maxSize: 50
            );
        }
        
        public List<UnexpectedTokenSyntaxError<IN>> GetErrorList()
        {
            return _errorListPool.Get();
        }
        
        public void ReturnErrorList(List<UnexpectedTokenSyntaxError<IN>> list)
        {
            _errorListPool.Return(list);
        }

        private string GetKey(IClause<IN, OUT> clause, int position)
        {
            return $"{clause.Dump()} -- @{position}";
        }
        
        public void Memoize(IClause<IN, OUT> clause, int position, SyntaxParseResult<IN, OUT> result)
        {
            if (_useMemoization)
            {
                _memoizedNonTerminalResults[GetKey(clause, position)] = result;
            }
        }

        public bool TryGetParseResult(IClause<IN, OUT> clause, int position, out SyntaxParseResult<IN, OUT> result)
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