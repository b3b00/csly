using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.strategies;

public class OptionParseStrategy<IN, OUT> : AbstractClauseParseStrategy<IN, OUT> where IN : struct
{
    public override SyntaxParseResult<IN> Parse(Rule<IN> rule, IClause<IN> clause, IList<Token<IN>> tokens, int position, SyntaxParsingContext<IN> parsingContext)
    {
        OptionClause<IN> optionClause = clause as OptionClause<IN>;
        
        if (parsingContext.TryGetParseResult(optionClause, position, out var parseResult))
        {
            return parseResult;
        }
        var result = new SyntaxParseResult<IN>();
        var currentPosition = position;
        var innerClause = optionClause.Clause;

        SyntaxParseResult<IN> innerResult = null;

        Strategist.Parse(innerClause, tokens, position, parsingContext);
       
        
        if (innerResult.IsError)
        {
            if (optionClause.IsTerminalOption())
            {
                result = new SyntaxParseResult<IN>();
                result.IsError = true;
                result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(),false);
                result.EndingPosition = position;   
            }
            else if (optionClause.IsNonTerminalOption())
            {
                result = new SyntaxParseResult<IN>();
                result.IsError = false;
                result.Root = new SyntaxEpsilon<IN>();
                result.EndingPosition = position;
            }
            else
            {
                result = new SyntaxParseResult<IN>();
                result.IsError = true;
                var children = new List<ISyntaxNode<IN>> {innerResult.Root};
                if (innerResult.IsError) children.Clear();
                result.Root = new OptionSyntaxNode<IN>( rule.NonTerminalName, children,
                    rule.GetVisitor());
                (result.Root as OptionSyntaxNode<IN>).IsGroupOption = optionClause.IsGroupOption;
                result.EndingPosition = position;
            }
           
        }
        else
        {
            var children = new List<ISyntaxNode<IN>> {innerResult.Root};
            result.Root =
                new OptionSyntaxNode<IN>( rule.NonTerminalName,children, rule.GetVisitor());
            result.EndingPosition = innerResult.EndingPosition;
            result.HasByPassNodes = innerResult.HasByPassNodes;
        }

        parsingContext.Memoize(optionClause, position, result);
        return result;
    }
}