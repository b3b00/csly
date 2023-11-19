using sly.lexer;
using sly.parser;
using sly.parser.generator;
using System.Linq.Expressions;

namespace ParserTests.Issue311;

public class Parser311
{
    public static Parser<Token311, Expression> GetParser()
    {
        var builder = new ParserBuilder<Token311, Expression>();
        var result = builder.BuildParser(new Parser311(), ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(Parser311) + "_expressions");
        return result.Result;
    }

    [Operand]
    [Production("operand: STRING")]
    public Expression LiteralString(Token<Token311> value)
        => Expression.Constant(value.StringWithoutQuotes);

    [Operand]
    [Production("operand: DOUBLE")]
    public Expression LiteralDouble(Token<Token311> value)
        => Expression.Constant(value.DoubleValue);

    [Operation((int)Token311.EQ, Affix.InFix, Associativity.Left, 1)]
    public Expression BinaryOperator(Expression left, Token<Token311> operation, Expression right)
        => Expression.Equal(left, right);
}

