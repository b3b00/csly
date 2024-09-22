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

    protected Match<TIn,TOut> MatchToken(IList<Token<TIn>> tokens, int position, bool discarded = false, params TIn[] expectedTokens)
    {
        if (expectedTokens.Contains(tokens[position].TokenID))
        {
            var leaf = new SyntaxLeaf<TIn, TOut>(tokens[position], discarded);
            return new Match<TIn,TOut>(leaf, position+1);
        }

        return new Match<TIn, TOut>();
    }

    protected SimpleParser<TIn,TOut> TerminalParser(string nodeName = null, bool discarded = false, Func<object[],  TOut> visitor = null,params TIn[] expectedTokens)
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
    
    protected SimpleParser<TIn,TOut> ZeroOrMore(string name, SimpleParser<TIn, TOut> repeatedParser) {
        // TODO x*
        SimpleParser<TIn,TOut> zeroOrMore =  (IList<Token<TIn>> tokens, int position) => {
            ManySyntaxNode<TIn,TOut> node = new ManySyntaxNode<TIn,TOut>(name);
            int currentPosition = position;
            var match = repeatedParser(tokens,currentPosition);
            while(currentPosition < tokens.Count && match.Matched) {
                node.Children.Add(match.Node);
                currentPosition = match.Position;
                match = repeatedParser(tokens,currentPosition);
            }
            return new Match<TIn, TOut>(node, currentPosition);
        };
        return  zeroOrMore;
        }
    
    protected SimpleParser<TIn,TOut> OneOrMore(string name, IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut> repeatedParser) {
            // TODO x+
            SimpleParser<TIn,TOut> oneOrMore =  (IList<Token<TIn>> tokens, int position) => {
                
                        int currentPosition = position;
                        var match = repeatedParser(tokens,currentPosition);
                        if (!match.Matched) {
                            return new Match<TIn, TOut>();
                        }                
                        ManySyntaxNode<TIn,TOut> node = new ManySyntaxNode<TIn,TOut>(name);

                        while(currentPosition < tokens.Count && match.Matched) {
                            node.Children.Add(match.Node);
                            currentPosition = match.Position;
                            match = repeatedParser(tokens,currentPosition);
                        }
                        return new Match<TIn, TOut>(node, currentPosition);
                    };
                    return  oneOrMore;
            }
    
    protected SimpleParser<TIn,TOut> Option(string name, IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut> optionalParser) {
            // TODO x?
            SimpleParser<TIn,TOut> option =  (IList<Token<TIn>> tokens, int position) => {
            OptionSyntaxNode<TIn, TOut> node = new OptionSyntaxNode<TIn, TOut>(name);
                var match = optionalParser(tokens,position);
                int newPosition = position;
                if (match.Matched) {
                    node.Children.Add(match.Node);
                    newPosition = match.Position;
                    }
                return new Match<TIn, TOut>(node, newPosition);
            };
            return option;
        }
    
    protected SimpleParser<TIn,TOut> Group(IList<Token<TIn>> tokens, int position, SimpleParser<TIn, TOut>[] groupedParser) {
            // TODO ( x Y z)
            return null;
            }
    
    // TODO : discarded tokens
    // TODO : explici tokens
}