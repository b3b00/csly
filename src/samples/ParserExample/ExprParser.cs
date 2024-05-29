using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

[ParserRoot("root")]
public class ExprParser
{
    [Production("root : ExprParser_expressions")]
    public object root___WhileParser_expressions(object p0)
    {
        return default(object);
    }

    [Infix("PLUS", Associativity.Right, 10)]
    public object PLUS(object left, Token<ExprLexer> oper, object right)
    {
        return left;
    }

    [Infix("MINUS", Associativity.Left, 10)]
    public object MINUS(object left, Token<ExprLexer> oper, object right)
    {
        return left;
    }

    [Infix("TIMES", Associativity.Right, 50)]
    public object TIMES(object left, Token<ExprLexer> oper, object right)
    {
        return left;
    }

    [Infix("DIVIDE", Associativity.Left, 50)]
    public object DIVIDE(object left, Token<ExprLexer> oper, object right)
    {
        return left;
    }

    [Prefix("MINUS", Associativity.Left, 100)]
    public object MINUS(Token<ExprLexer> oper, object value)
    {
        return value;
    }

    [Operand]
    [Production("operand : NUMBER")]
    public object operand___NUMBER(Token<ExprLexer> p0)
    {
        return default(object);
    }

    [Operand]
    [Production("operand : LPAREN ExprParser_expressions RPAREN")]
    public object operand___LPAREN_WhileParser_expressions_RPAREN(Token<ExprLexer> p0, object p1, Token<ExprLexer> p2)
    {
        return default(object);
    }
}