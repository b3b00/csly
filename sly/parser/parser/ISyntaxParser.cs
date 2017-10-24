using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;

namespace sly.parser
{

    public interface ISyntaxParser<IN,OUT> where IN : struct
    {

        string StartingNonTerminal { get; set; }

        SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null);

        void Init(ParserConfiguration<IN, OUT> configuration);

    }
}