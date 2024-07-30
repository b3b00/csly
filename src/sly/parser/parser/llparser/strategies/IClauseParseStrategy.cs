using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.strategies;

public interface IClauseParseStrategy<IN, OUT> where IN : struct
{
    Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }

    ParserConfiguration<IN, OUT> Configuration { get; set; }

    public string I18n { get; set; }
    
    IParseStrategist<IN, OUT> Strategist { get; set; }

    SyntaxParseResult<IN> Parse(IClause<IN> clause, IList<Token<IN>> tokens, int position,
        SyntaxParsingContext<IN> parsingContext);

    
}

