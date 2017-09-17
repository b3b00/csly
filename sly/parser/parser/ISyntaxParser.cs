using sly.lexer;
using System.Collections.Generic;

namespace sly.parser
{

    public interface ISyntaxParser<IN>
    {
        
        SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null);


    }
}