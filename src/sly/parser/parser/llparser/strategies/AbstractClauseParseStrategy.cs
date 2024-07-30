using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.strategies;

public abstract class AbstractClauseParseStrategy<IN, OUT> : IClauseParseStrategy<IN, OUT> where IN : struct
{
    public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }
    public string I18n { get; set; }
    public IParseStrategist<IN, OUT> Strategist { get; set; }

    public ParserConfiguration<IN, OUT> Configuration { get; set; }

    public abstract SyntaxParseResult<IN> Parse(Rule<IN> rule, IClause<IN> clause, IList<Token<IN>> tokens, int position,
        SyntaxParsingContext<IN> parsingContext);
    
    protected SyntaxParseResult<IN> NoMatchingRuleError(IList<Token<IN>> tokens, int currentPosition,
        List<LeadingToken<IN>> allAcceptableTokens)
    {
        var noRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();

        if (currentPosition < tokens.Count)
        {
            noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], I18n,
                allAcceptableTokens));
        }
        else
        {
            noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(new Token<IN> { IsEOS = true }, I18n,
                allAcceptableTokens));
        }

        var error = new SyntaxParseResult<IN>();
        error.IsError = true;
        error.Root = null;
        error.IsEnded = false;
        error.Errors = noRuleErrors;
        error.EndingPosition = currentPosition;
        error.Expecting = allAcceptableTokens;

        return error;
    }
    
}