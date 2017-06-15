using sly.lexer;
using System.Collections.Generic;

namespace sly.parser
{

    public interface ISyntaxParser<T>
    {
        
        SyntaxParseResult<T> Parse(IList<Token<T>> tokens, string startingNonTerminal = null);


    }
}