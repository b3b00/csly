using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.strategies;

public class ChoiceParseStrategy<IN, OUT> : AbstractClauseParseStrategy<IN, OUT> where IN : struct
{
    public  override SyntaxParseResult<IN> Parse(Rule<IN> rule, IClause<IN> clause, IList<Token<IN>> tokens, int position,
        SyntaxParsingContext<IN> parsingContext)
    {
        var choiceClause = clause as ChoiceClause<IN>;
            
        if (parsingContext.TryGetParseResult(choiceClause, position, out var parseResult))
        {
            return parseResult;
        }
        var currentPosition = position;

        SyntaxParseResult<IN> result = new SyntaxParseResult<IN>
        {
            IsError = true,
            IsEnded = false,
            EndingPosition = currentPosition
        };
             

        foreach (var alternate in choiceClause.Choices)
        {
            result = Strategist.Parse(alternate, tokens, position, parsingContext);

            if (result.IsOk)
            {
                if (choiceClause.IsTerminalChoice && choiceClause.IsDiscarded && result.Root is SyntaxLeaf<IN> leaf)
                {
                    var discardedToken = new SyntaxLeaf<IN>(leaf.Token, true);
                    result.Root = discardedToken;
                }
                parsingContext.Memoize(choiceClause,position,result);
                return result;
            }
        }

        if (result.IsError && choiceClause.IsTerminalChoice)
        {
            var terminalAlternates = choiceClause.Choices.Cast<TerminalClause<IN>>();
            var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
            result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,expected.ToArray()));
        }
        parsingContext.Memoize(choiceClause,position,result);
        return result;
    }
}