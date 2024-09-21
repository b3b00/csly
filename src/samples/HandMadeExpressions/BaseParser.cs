using simpleExpressionParser;
using sly.lexer;
using sly.parser.syntax.tree;
using System.Linq;

namespace handExpressions;


public delegate Match<TIn,TOut> SimpleParser<TIn, TOut>(IList<Token<TIn>> tokens, int position) where TIn : struct;

public class BaseParser<TIn,TOut> where TIn : struct
{
    public BaseParser()
    {
    }

    protected Match<TIn,TOut> MatchToken(IList<Token<TIn>> tokens, int position, params TIn[] expectedTokens)
    {
        if (expectedTokens.Contains(tokens[position].TokenID))
        {
            var leaf = new SyntaxLeaf<TIn, TOut>(tokens[position], false);
            return new Match<TIn,TOut>(leaf, position+1);
        }

        return new Match<TIn, TOut>();
    }

    protected SimpleParser<TIn,TOut> TerminalParser(string nodeName = null, Func<object[], TOut> visitor = null,params TIn[] expectedTokens)
    {
        return (tokens, position) =>
        {
            if (expectedTokens.Contains(tokens[position].TokenID))
            {
                if (!string.IsNullOrEmpty(nodeName) && visitor != null)
                {
                    var node = new SyntaxNode<TIn, TOut>("primary",
                        new List<ISyntaxNode<TIn, TOut>>()
                        {
                            new SyntaxLeaf<TIn, TOut>(tokens[position], false)
                        });
                    node.LambdaVisitor = visitor;
                    return new Match<TIn, TOut>(node, position + 1);
                }
                var leaf = new SyntaxLeaf<TIn, TOut>(tokens[position], false);
                return new Match<TIn, TOut>(leaf, position + 1);
            }

            return new Match<TIn, TOut>();
        };
    }

    protected Match<TIn, TOut> MatchNonTerminal(IList<Token<TIn>> tokens,
        int position, params SimpleParser<TIn,TOut>[] parsers)
    {

        for (int i = 0; i < parsers.Length; i++)
        {
            var parser = parsers[i];
            var match = parser(tokens, position);
            if (match.Matched)
            {
                return match;
            }
        }
        return new Match<TIn, TOut>();
    }

    protected Match<TIn, TOut> MatchSequence(string? nodeName,  Func<object[],TOut> visitor, IList<Token<TIn>> tokens,
        int position, params SimpleParser<TIn,TOut>[] clauses)
    {
        var node = new SyntaxNode<TIn,TOut>(nodeName);
        node.LambdaVisitor = visitor;
        bool ok = true; 
        int i = 0;
        int currentPosition = position;
        while (i < clauses.Length && ok)
        {
            var currentClause = clauses[i] as SimpleParser<TIn,TOut>;
            var match = currentClause(tokens, currentPosition);
            ok = match.Matched;
            node.Children.Add(match.Node);
            currentPosition = match.Position;
            i++;
        }

        if (ok)
        {
            return new Match<TIn, TOut>(node,currentPosition);
        }
        return new Match<TIn, TOut>();
    }

    protected Match<TIn, TOut> MatchInfix(string nodeName, Func<object[], TOut> visitor,
        IList<Token<TIn>> tokens,
        int position, TIn[] operators, SimpleParser<TIn,TOut> left, SimpleParser<TIn,TOut> right)
    {
        var node = new SyntaxNode<TIn, TOut>(nodeName);
        node.LambdaVisitor = visitor;
        
        
        var leftMatch = left(tokens, position);
        if (!leftMatch.Matched)
        {
            return new Match<TIn, TOut>();
        }
        node.Children.Add(leftMatch.Node);

        var operatorpParser = TerminalParser(expectedTokens: operators);
        var operatorMatch = operatorpParser(tokens, leftMatch.Position);
        if (!operatorMatch.Matched)
        {
            var x = new SyntaxNode<TIn, TOut>(nodeName);
            x.Children.Add(leftMatch.Node);
            x.IsByPassNode = true;
            return new Match<TIn, TOut>(x, leftMatch.Position);
            //return new Match<TIn, TOut>();
        }
        node.Children.Add(operatorMatch.Node);
        
        var rightMatch = right(tokens, leftMatch.Position + 1);
        if (!rightMatch.Matched)
        {
            return new Match<TIn, TOut>();
        }
        node.Children.Add(rightMatch.Node);

        return new Match<TIn, TOut>(node, rightMatch.Position);

    }
    
    protected SimpleParser<TIn,TOut> ZeroOrMore(IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut> repeatedParser) {
        // TODO x*
        return null;
        }
    
    protected SimpleParser<TIn,TOut> OneOrMore(IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut> repeatedParser) {
            // TODO x+
            return null;
            }
    
    protected SimpleParser<TIn,TOut> Option(IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut> optionalParser) {
            // TODO x?
            return null;
            }
    
    protected SimpleParser<TIn,TOut> Group(IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut>[] groupedParser) {
            // TODO ( x Y z)
            return null;
            }
    
    // TODO : discarded tokens
    // TODO : explici tokens
}