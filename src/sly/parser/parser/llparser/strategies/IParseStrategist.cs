using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.strategies;

public interface IParseStrategist<IN, OUT> where IN : struct
{
    SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position,
        string nonTerminalName, SyntaxParsingContext<IN> parsingContext);

    SyntaxParseResult<IN> Parse(IClause<IN> clause, IList<Token<IN>> tokens, int position,
        SyntaxParsingContext<IN> parsingContext);
}