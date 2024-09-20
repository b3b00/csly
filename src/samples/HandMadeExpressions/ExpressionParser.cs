using Microsoft.CodeAnalysis.Differencing;
using simpleExpressionParser;
using sly.lexer;
using sly.parser.syntax.tree;

namespace handExpressions;

public class Match<I, O> where I : struct
{

    public int Position = 0;
    
    public bool Matched = false;

    public ISyntaxNode<I, O> Node;
    public Match(ISyntaxNode<I, O> node, int position)
    {
        Position = position;
        Node = node;
        Matched = true;
    }

    public Match()
    {
        Matched = false;
    }
}

public delegate Match<GenericExpressionToken,double> SimpleParser(IList<Token<GenericExpressionToken>> tokens, int position);

public class ExpressionParser
{

    GenericSimpleExpressionParser Instance = null;


    private Match<GenericExpressionToken,double> MatchToken(IList<Token<GenericExpressionToken>> tokens, int position, params GenericExpressionToken[] expectedTokens)
    {
        if (expectedTokens.Contains(tokens[position].TokenID))
        {
            var leaf = new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false);
            return new Match<GenericExpressionToken,double>(leaf, position+1);
        }

        return new Match<GenericExpressionToken, double>();
    }


    private SimpleParser TerminalParser(string nodeName = null, Func<object[], double> visitor = null,params GenericExpressionToken[] expectedTokens)
    {
        return (tokens, position) =>
        {
            if (expectedTokens.Contains(tokens[position].TokenID))
            {
                if (!string.IsNullOrEmpty(nodeName) && visitor != null)
                {
                    var node = new SyntaxNode<GenericExpressionToken, double>("primary",
                        new List<ISyntaxNode<GenericExpressionToken, double>>()
                        {
                            new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false)
                        });
                    node.LambdaVisitor = visitor;
                    return new Match<GenericExpressionToken, double>(node, position + 1);
                }
                var leaf = new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false);
                return new Match<GenericExpressionToken, double>(leaf, position + 1);
            }

            return new Match<GenericExpressionToken, double>();
        };
    }


    private Match<GenericExpressionToken, double> MatchNonTerminal(IList<Token<GenericExpressionToken>> tokens,
        int position, params SimpleParser[] parsers)
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
        return new Match<GenericExpressionToken, double>();
    }

    private Match<GenericExpressionToken, double> MatchSequence(string? nodeName,  Func<object[],double> visitor, IList<Token<GenericExpressionToken>> tokens,
        int position, params SimpleParser[] clauses)
    {
        var node = new SyntaxNode<GenericExpressionToken,double>(nodeName);
        node.LambdaVisitor = visitor;
        bool ok = true; 
        int i = 0;
        int currentPosition = position;
        while (i < clauses.Length && ok)
        {
            var currentClause = clauses[i] as SimpleParser;
            var match = currentClause(tokens, currentPosition);
            ok = match.Matched;
            node.Children.Add(match.Node);
            currentPosition = match.Position;
            i++;
        }

        if (ok)
        {
            return new Match<GenericExpressionToken, double>(node,currentPosition);
        }
        return new Match<GenericExpressionToken, double>();
    }

    private Match<GenericExpressionToken, double> MatchInfix(string nodeName, Func<object[], double> visitor,
        IList<Token<GenericExpressionToken>> tokens,
        int position, GenericExpressionToken[] operators, SimpleParser left, SimpleParser right)
    {
        var node = new SyntaxNode<GenericExpressionToken, double>(nodeName);
        node.LambdaVisitor = visitor;
        
        
        var leftMatch = left(tokens, position);
        if (!leftMatch.Matched)
        {
            return new Match<GenericExpressionToken, double>();
        }
        node.Children.Add(leftMatch.Node);

        var operatorpParser = TerminalParser(expectedTokens: operators);
        var operatorMatch = operatorpParser(tokens, leftMatch.Position);
        if (!operatorMatch.Matched)
        {
            var x = new SyntaxNode<GenericExpressionToken, double>(nodeName);
            x.Children.Add(leftMatch.Node);
            x.IsByPassNode = true;
            return new Match<GenericExpressionToken, double>(x, leftMatch.Position);
            //return new Match<GenericExpressionToken, double>();
        }
        node.Children.Add(operatorMatch.Node);
        
        var rightMatch = right(tokens, leftMatch.Position + 1);
        if (!rightMatch.Matched)
        {
            return new Match<GenericExpressionToken, double>();
        }
        node.Children.Add(rightMatch.Node);

        return new Match<GenericExpressionToken, double>(node, rightMatch.Position);

    }
    
    public ExpressionParser(GenericSimpleExpressionParser instance)
    {
        Instance = instance;
    }
    
    public Match<GenericExpressionToken,double> Root(IList<Token<GenericExpressionToken>> tokens,  int position )
    {
        var match = Expression(tokens,position);
        var node = new SyntaxNode<GenericExpressionToken, double>("expression",
            new List<ISyntaxNode<GenericExpressionToken, double>>() { match.Node });
        node.LambdaVisitor = args =>
        {
            return Instance.Root((double)args[0]);
        };
        return new Match<GenericExpressionToken, double>(node, match.Position);
    }
    
#region binaries



public Match<GenericExpressionToken, double> TimesDiv(IList<Token<GenericExpressionToken>> tokens, int position)
{
    var m = MatchInfix("timesdiv",args =>
    {
        return Instance.BinaryFactorExpression((double)args[0], (Token<GenericExpressionToken>)args[1], (double)args[2]);
    }, tokens, position,new[] { GenericExpressionToken.TIMES , GenericExpressionToken.DIVIDE},MinusFact,TimesDiv);
    return m;
    
}


public Match<GenericExpressionToken, double> PlusMinus(IList<Token<GenericExpressionToken>> tokens, int position)
{
    var m = MatchInfix("timesdiv",args =>
    {
        return Instance.BinaryTermExpression((double)args[0], (Token<GenericExpressionToken>)args[1], (double)args[2]);
    }, tokens, position,new[] { GenericExpressionToken.PLUS , GenericExpressionToken.MINUS},TimesDiv,PlusMinus);
    return m;
}

#endregion

public Match<GenericExpressionToken, double> Expression(IList<Token<GenericExpressionToken>> tokens, int position)
{
    return PlusMinus(tokens, position);
}


#region operands 
    public Match<GenericExpressionToken, double> Operand(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        if (position == 7)
        {
            ;
        }
        var match = PrimaryValue(tokens, position);
        var node = new SyntaxNode<GenericExpressionToken, double>("operand",
            new List<ISyntaxNode<GenericExpressionToken, double>>() { match.Node });
        node.LambdaVisitor = args =>
        {
            return (double)args[0];
            return Instance.OperandValue((double)args[0]);
        };
        return new Match<GenericExpressionToken, double>(node, match.Position);
    }

    public Match<GenericExpressionToken,double> PrimaryValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        return MatchNonTerminal(tokens, position, DoubleValue, IntegerValue, GroupExpression, PrimaryTernary);
    }

    public Match<GenericExpressionToken,double> DoubleValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        var parser = TerminalParser("primary", args =>
        {
            return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
        } , GenericExpressionToken.DOUBLE);
        return parser(tokens, position);
        //
        // if (tokens[position].TokenID == GenericExpressionToken.DOUBLE)
        // {
        //     var node = new SyntaxNode<GenericExpressionToken, double>("primary",
        //         new List<ISyntaxNode<GenericExpressionToken, double>>()
        //         {
        //             new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false)
        //         });
        //     node.LambdaVisitor = args =>
        //     {
        //         return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
        //     };
        //     return new Match<GenericExpressionToken, double>(node, position + 1);
        // }
        //
        // return new Match<GenericExpressionToken, double>();
    }
    public Match<GenericExpressionToken,double> IntegerValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        
        var parser = TerminalParser("primary", args =>
        {
            return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
        } , GenericExpressionToken.INT);
        return parser(tokens, position);
        
        // if (tokens[position].TokenID == GenericExpressionToken.INT)
        // {
        //     var node = new SyntaxNode<GenericExpressionToken, double>("primary",
        //         new List<ISyntaxNode<GenericExpressionToken, double>>()
        //         {
        //             new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false)
        //         });
        //     node.LambdaVisitor = args =>
        //     {
        //         return Instance.OperandInt((Token<GenericExpressionToken>)args[0]);
        //     };
        //     return new Match<GenericExpressionToken, double>(node, position + 1);
        // }
        //
        // return new Match<GenericExpressionToken, double>();
    }


    public Match<GenericExpressionToken, double> GroupExpression(IList<Token<GenericExpressionToken>> tokens,
        int position)
    {
        Func<object[],double> visitor = (object[] args) =>
        {
            return Instance.OperandParens((Token<GenericExpressionToken>)args[0], (double)args[1],
                (Token<GenericExpressionToken>)args[2]);
        };
        var openParen = TerminalParser(expectedTokens:GenericExpressionToken.LPAREN);
        var closeParen = TerminalParser(expectedTokens:GenericExpressionToken.RPAREN);
        return MatchSequence("group",visitor, tokens, position, openParen, Expression, closeParen);
    }   

    public Match<GenericExpressionToken, double> PrimaryTernary(IList<Token<GenericExpressionToken>> tokens,
        int position)
    {
        SyntaxNode<GenericExpressionToken, double> node = new SyntaxNode<GenericExpressionToken, double>("ternary");
        node.LambdaVisitor = args =>
        {
            return Instance.Ternary((Token<GenericExpressionToken>)args[0], (double)args[2], (double)args[4]);
        };

        var condition = TerminalParser(expectedTokens:new[]{
            GenericExpressionToken.TRUE, GenericExpressionToken.FALSE
        })
        ;
        var question = TerminalParser(expectedTokens:GenericExpressionToken.QUESTION);
        var colon = TerminalParser(expectedTokens:GenericExpressionToken.COLON);
        return MatchSequence("ternary",args =>
        {
            return Instance.Ternary((Token<GenericExpressionToken>)args[0], (double)args[2], (double)args[4]);
        },
            tokens, position, condition, question, Expression, colon, Expression);
    }

    #endregion
    
    #region minus factorial


    public Match<GenericExpressionToken, double> MinusFact(IList<Token<GenericExpressionToken>> tokens,
        int position)
    {
        var node = new SyntaxNode<GenericExpressionToken, double>("toBeNamed",
            new List<ISyntaxNode<GenericExpressionToken, double>>());
        
        
        bool isMinus = false;
        if (tokens[position].TokenID == GenericExpressionToken.MINUS)
        {
            node.Children.Add(new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false));
            isMinus = true;
        }
        var match = Operand(tokens, position+(isMinus ? 1 : 0));
        if (!match.Matched)
        {
            return new Match<GenericExpressionToken, double>();
        }
        else
        {
            node.Children.Add(match.Node);
            if (isMinus)
            {
                node.Name = "minus_factorial";
                // prefix minus operation
                node.LambdaVisitor = args =>
                {
                    return Instance.PreFixExpression((Token<GenericExpressionToken>)args[0], (double)args[1]);
                };
                return new Match<GenericExpressionToken, double>(node, match.Position);
            }
            bool isFactorial = false;
            if (match.Position < tokens.Count-1 && tokens[match.Position].TokenID == GenericExpressionToken.FACTORIAL)
            {
                node.Name = "minus_factorial";
                isFactorial = true;
                node.Children.Add(new SyntaxLeaf<GenericExpressionToken, double>(tokens[match.Position], false));
                node.LambdaVisitor = args =>
                {
                    return Instance.PostFixExpression((double)args[0], (Token<GenericExpressionToken>)args[1]);
                };
                return new Match<GenericExpressionToken, double>(node, match.Position+1);
            }

            node.IsByPassNode = true;
            node.Name = "minus_factorial";
            return new Match<GenericExpressionToken, double>(node, match.Position);

        }
        

        return new Match<GenericExpressionToken, double>();
    }


  
   
    
    #endregion
    
}