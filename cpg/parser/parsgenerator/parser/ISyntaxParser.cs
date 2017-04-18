using System.Collections.Generic;

namespace parser.parsergenerator.parser
{

    public interface ISyntaxParser<T>
    {

        SyntaxParseResult<T> Parse(IList<T> tokens);

    }
}