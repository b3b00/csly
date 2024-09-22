using Microsoft.CodeAnalysis.Differencing;
using simpleExpressionParser;
using sly.lexer;
using sly.parser.syntax.tree;

namespace handExpressions;




public class ExpressionParser : BaseParser<GenericExpressionToken, double>
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
        var parser = TerminalParser("primary", visitor:args =>
        {
            return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
        } , expectedTokens: new []{GenericExpressionToken.DOUBLE});
        return parser(tokens, position);
       
    }
    public Match<GenericExpressionToken,double> IntegerValue(IList<Token<GenericExpressionToken>> tokens, int position)
    {
        
        var parser = TerminalParser("primary", visitor:args =>
        {
            return Instance.OperandDouble((Token<GenericExpressionToken>)args[0]);
        } , expectedTokens : new []{GenericExpressionToken.INT});
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
            return Instance.OperandParens((double)args[1]);
        };
        var openParen = DiscardedTerminalParser(expectedTokens:GenericExpressionToken.LPAREN);
        var closeParen = DiscardedTerminalParser(expectedTokens:GenericExpressionToken.RPAREN);
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
        var question = DiscardedTerminalParser(expectedTokens:GenericExpressionToken.QUESTION);
        var colon = DiscardedTerminalParser(expectedTokens:GenericExpressionToken.COLON);
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