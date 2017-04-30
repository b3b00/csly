using lexer;
using System.Collections.Generic;

namespace parser.parsergenerator.parser
{

    public interface ISyntaxParser<T>
    {

        SyntaxParseResult<T> Parse(IList<Token<T>> tokens);


    }
}