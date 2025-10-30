using System;
using System.Collections.Generic;
using sly.parser.parser;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    
    public class SyntaxParsingContext<IN, OUT> where IN : struct, Enum
    {
        // Optimization: Use LRU cache instead of unlimited Dictionary for better memory management
        private readonly LruCache<string, SyntaxParseResult<IN, OUT>> _memoizedNonTerminalResults;
        
        // Optimization: Pool for error lists to reduce allocations
        private readonly ObjectPool<List<UnexpectedTokenSyntaxError<IN>>> _errorListPool;

        private readonly bool _useMemoization;
        
        private const int DefaultCacheCapacity = 1000;
        
        public SyntaxParsingContext(bool useMemoization, int cacheCapacity = DefaultCacheCapacity)
        {
            _useMemoization = useMemoization;
            
            // Initialize LRU cache only if memoization is enabled
            if (useMemoization)
            {
                _memoizedNonTerminalResults = new LruCache<string, SyntaxParseResult<IN, OUT>>(cacheCapacity);
            }
            
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
            if (_useMemoization && _memoizedNonTerminalResults != null)
            {
                _memoizedNonTerminalResults.Set(GetKey(clause, position), result);
            }
        }

        public bool TryGetParseResult(IClause<IN, OUT> clause, int position, out SyntaxParseResult<IN, OUT> result)
        {
            if (!_useMemoization || _memoizedNonTerminalResults == null)
            {
                result = null;
                return false;
            }
            
            return _memoizedNonTerminalResults.TryGetValue(GetKey(clause, position), out result);
        }
        
        /// <summary>
        /// Clear the memoization cache (useful for freeing memory between large parsing operations)
        /// </summary>
        public void ClearCache()
        {
            _memoizedNonTerminalResults?.Clear();
        }
        
        /// <summary>
        /// Get cache statistics
        /// </summary>
        public (int count, int capacity) GetCacheStats()
        {
            if (_memoizedNonTerminalResults == null)
                return (0, 0);
            
            return (_memoizedNonTerminalResults.Count, _memoizedNonTerminalResults.Capacity);
        }
    }
}