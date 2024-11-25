using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.bnf;

public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    #region parsing

    public SyntaxParseResult<IN> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN> terminal, int position,
        SyntaxParsingContext<IN> parsingContext)
    {
        if (parsingContext.TryGetParseResult(terminal, position, out var parseResult))
        {
            return parseResult;
        }

        var result = new SyntaxParseResult<IN>();
        result.IsError = !terminal.Check(tokens[position]);
        result.EndingPosition = !result.IsError ? position + 1 : position;
        var token = tokens[position];
        token.Discarded = terminal.Discarded;
        token.IsExplicit = terminal.IsExplicitToken;
        result.Root = new SyntaxLeaf<IN>(token, terminal.Discarded);
        result.HasByPassNodes = false;
        if (result.IsError)
        {
            result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
            
            result.AddExpecting(terminal.ExpectedToken);
        }

        parsingContext.Memoize(terminal, position, result);
        return result;
    }

    #endregion
}