using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    
    public class SyntaxParsingContext<IN, OUT> where IN : struct
    {

        private DefaultObjectPoolProvider _poolProvider = new DefaultObjectPoolProvider();
        
        private ObjectPool<UnexpectedTokenSyntaxError<IN>> _unexpectedErrorPool;
        
        private readonly Dictionary<string, SyntaxParseResult<IN, OUT>> _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN, OUT>>();

        private readonly bool _useMemoization = false;
        public SyntaxParsingContext(bool useMemoization)
        {
            _unexpectedErrorPool = _poolProvider.Create<UnexpectedTokenSyntaxError<IN>>();
            _useMemoization = useMemoization;
        }

        public UnexpectedTokenSyntaxError<IN> GetError(Token<IN> unexpectedToken, Dictionary<IN, Dictionary<string, string>> labels, string i18n=null, params LeadingToken<IN>[] expectedTokens )
        {
            var error = _unexpectedErrorPool.Get();
            error.Init(unexpectedToken,labels, i18n, expectedTokens);
            return error;
        }

        public void ReleaseError(UnexpectedTokenSyntaxError<IN> error)
        {
            _unexpectedErrorPool.Return(error);
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