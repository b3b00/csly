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

public class ExpressionParser
{

    GenericSimpleExpressionParser Instance = null; 
    
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
    var match = MinusFact(tokens, position);
    if (!match.Matched)
    {
        return new Match<GenericExpressionToken, double>();
    }

    var node1 = match.Node;
    var token = tokens[match.Position];
    if (token.TokenID != GenericExpressionToken.TIMES && token.TokenID != GenericExpressionToken.DIVIDE)
    {
        return match;
    }

    match = TimesDiv(tokens, match.Position + 1);
    if (!match.Matched)
    {
        return new Match<GenericExpressionToken, double>();
    }

    var node = new SyntaxNode<GenericExpressionToken, double>("timesDiv",
        new List<ISyntaxNode<GenericExpressionToken, double>>()
        {
            node1,
            new SyntaxLeaf<GenericExpressionToken, double>(token, false),
            match.Node
        });

    node.LambdaVisitor = args =>
    {
        return Instance.BinaryFactorExpression((double)args[0], (Token<GenericExpressionToken>)args[1], (double)args[2]);
    };
    return new Match<GenericExpressionToken, double>(node, match.Position);
}


public Match<GenericExpressionToken, double> PlusMinus(IList<Token<GenericExpressionToken>> tokens, int position)
{
    var match = TimesDiv(tokens, position);
    if (!match.Matched)
    {
        return new Match<GenericExpressionToken, double>();
    }

    var node1 = match.Node;
    var token = tokens[match.Position];
    if (token.TokenID != GenericExpressionToken.MINUS && token.TokenID != GenericExpressionToken.PLUS)
    {
        return match;
    }

    match = PlusMinus(tokens, match.Position + 1);
    if (!match.Matched)
    {
        return new Match<GenericExpressionToken, double>();
    }

    var node = new SyntaxNode<GenericExpressionToken, double>("plusMinus",
        new List<ISyntaxNode<GenericExpressionToken, double>>()
        {
            node1,
            new SyntaxLeaf<GenericExpressionToken, double>(token, false),
            match.Node
        });
    
    node.LambdaVisitor = args =>
    {
        return Instance.BinaryTermExpression((double)args[0], (Token<GenericExpressionToken>)args[1], (double)args[2]);
    };
    return new Match<GenericExpressionToken, double>(node, match.Position);
}

#endregion

public Match<GenericExpressionToken, double> Expression(IList<Token<GenericExpressionToken>> tokens, int position)
{
    return PlusMinus(tokens, position);
}


#region operands 
    public Match<GenericExpressionToken, double> Operand(IList<Token<GenericExpressionToken>> tokens, int position)
    {
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
        var match = DoubleValue(tokens, position);
        if (match.Matched)
        {
            return match;
        }
        match = IntegerValue(tokens, position);
        if (match.Matched)
        {
            return match;
        }

        match = GroupValue(tokens, position);
        if (match.Matched)
        {
            return match;
        }
        
        return new Match<GenericExpressionToken, double>();
    }

    public Match<GenericExpressionToken,double> DoubleValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        if (tokens[position].TokenID == GenericExpressionToken.DOUBLE)
        {
            var node = new SyntaxNode<GenericExpressionToken, double>("primary",
                new List<ISyntaxNode<GenericExpressionToken, double>>()
                {
                    new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false)
                });
            node.LambdaVisitor = args =>
            {
                return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
            };
            return new Match<GenericExpressionToken, double>(node, position + 1);
        }

        return new Match<GenericExpressionToken, double>();
    }
    public Match<GenericExpressionToken,double> IntegerValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        if (tokens[position].TokenID == GenericExpressionToken.INT)
        {
            var node = new SyntaxNode<GenericExpressionToken, double>("primary",
                new List<ISyntaxNode<GenericExpressionToken, double>>()
                {
                    new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false)
                });
            node.LambdaVisitor = args =>
            {
                return Instance.OperandInt((Token<GenericExpressionToken>)args[0]);
            };
            return new Match<GenericExpressionToken, double>(node, position + 1);
        }

        return new Match<GenericExpressionToken, double>();
    }
    
    public Match<GenericExpressionToken,double> GroupValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        if (tokens[position].TokenID != GenericExpressionToken.LPAREN)
        {
            return new Match<GenericExpressionToken,double>();
        } 
        var match = Expression(tokens, position+1);
        if (!match.Matched)
        {
            return new Match<GenericExpressionToken,double>();
        }
        if (tokens[match.Position].TokenID != GenericExpressionToken.LPAREN)
        {
            return new Match<GenericExpressionToken,double>();
        }

        var node = new SyntaxNode<GenericExpressionToken, double>("group",
            new List<ISyntaxNode<GenericExpressionToken, double>>() { match.Node });
        node.LambdaVisitor = args =>
        {
            return Instance.OperandParens((Token<GenericExpressionToken>)args[0], (double)args[1],
                (Token<GenericExpressionToken>)args[2]);
        };
        return new Match<GenericExpressionToken, double>(node, match.Position+1);
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
    
    public Match<GenericExpressionToken, double> MinusFactorial(IList<Token<GenericExpressionToken>> tokens,
        int position)
    {
        if (tokens[position].TokenID != GenericExpressionToken.MINUS)
        {
            var operand = Operand(tokens, position);
            var node = new SyntaxNode<GenericExpressionToken, double>("prefix",
                new List<ISyntaxNode<GenericExpressionToken, double>>()
                {
                    new SyntaxLeaf<GenericExpressionToken, double>(tokens[position], false),
                    operand.Node
                });
            node.IsByPassNode = true;
            node.LambdaVisitor = args =>
            {
                return Instance.PreFixExpression((Token<GenericExpressionToken>)args[0], (double)args[1]);
            };
            return new Match<GenericExpressionToken, double>(node, operand.Position);

        }
        var match = Operand(tokens, position+1);
        if (!match.Matched)
        {
            return new Match<GenericExpressionToken, double>();
        }

        if (tokens[match.Position].TokenID == GenericExpressionToken.FACTORIAL)
        {
            var node = new SyntaxNode<GenericExpressionToken, double>("minus_factorial",
                new List<ISyntaxNode<GenericExpressionToken, double>>() { match.Node });
            node.LambdaVisitor = args =>
            {
                return Instance.PostFixExpression((double)args[0], (Token<GenericExpressionToken>)args[1]);
            };
            return new Match<GenericExpressionToken, double>(node, match.Position+1);
        }
        else
        {
            return match;
        }
        return new Match<GenericExpressionToken, double>();
    }
    
    #endregion
    
}