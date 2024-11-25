using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace SlowEOS;

[ParserRoot("root")]
public class SlowOnBadParseEos
{
    [Production("root : SlowOnBadParseEos_expressions")]
    public object root_SlowOnBadParseEosexpressions(object p0)
    {
        return default(object);
    }

    [Prefix("NOT", Associativity.Left, 92)]
    public object NOT(Token<SlowOnBadParseEosToken> oper, object value)
    {
        return value;
    }

    [Prefix("EXCLAMATION_POINT", Associativity.Left, 91)]
    public object EXCLAMATION_POINT(Token<SlowOnBadParseEosToken> oper, object value)
    {
        return value;
    }

    [Right("IN",  71)]
    public object IN(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Right("LIKE", 70)]
    public object LIKE(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("ARITH_TIMES", 40)]
    public object ARITH_TIMES(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("ARITH_DIVIDE", 40)]
    public object ARITH_DIVIDE(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("ARITH_MODULO", 38)]
    public object ARITH_MODULO(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("ARITH_PLUS", 30)]
    public object ARITH_PLUS(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("ARITH_MINUS", 30)]
    public object ARITH_MINUS(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Prefix("ARITH_MINUS", Associativity.Left, 29)]
    public object ARITH_MINUS(Token<SlowOnBadParseEosToken> oper, object value)
    {
        return value;
    }

    [Left("COMP_EQUALS", 25)]
    public object COMP_EQUALS(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("COMP_NOTEQUALS", 24)]
    public object COMP_NOTEQUALS(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("COMP_LT", 23)]
    public object COMP_LT(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("COMP_GT", 22)]
    public object COMP_GT(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("COMP_LTE", 21)]
    public object COMP_LTE(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("COMP_GTE", 20)]
    public object COMP_GTE(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("IS", 15)]
    public object IS(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("AND", 12)]
    public object AND(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("OR", 11)]
    public object OR(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Left("XOR", 10)]
    public object XOR(object left, Token<SlowOnBadParseEosToken> oper, object right)
    {
        return left;
    }

    [Operand]
    [Production("primary : BRACKET_LEFT IDENTIFIER BRACKET_RIGHT")]
    public object primary_BRACKETLEFT_IDENTIFIER_BRACKETRIGHT(Token<SlowOnBadParseEosToken> p0, Token<SlowOnBadParseEosToken> p1, Token<SlowOnBadParseEosToken> p2)
    {
        return default(object);
    }

    [Operand]
    [Production("primary : PARENS_LEFT SlowOnBadParseEos_expressions PARENS_RIGHT")]
    public object primary_PARENSLEFT_SlowOnBadParseEosexpressions_PARENSRIGHT(Token<SlowOnBadParseEosToken> p0, object p1, Token<SlowOnBadParseEosToken> p2)
    {
        return default(object);
    }

    [Operand]
    [Production("literal : INT")]
    public object literal_INT(Token<SlowOnBadParseEosToken> p0)
    {
        return default(object);
    }

    [Operand]
    [Production("literal : NUMBER")]
    public object literal_NUMBER(Token<SlowOnBadParseEosToken> p0)
    {
        return default(object);
    }

    [Operand]
    [Production("literal : STRING")]
    public object literal_STRING(Token<SlowOnBadParseEosToken> p0)
    {
        return default(object);
    }

    // [Operand]	
    [Production("literal : [ TRUE | FALSE ]")]
    public object literal_TRUE_FALSE_(Token<SlowOnBadParseEosToken> p0)
    {
        return default(object);
    }

    [Production("literal : NULL")]
    public object literal_NULL(Token<SlowOnBadParseEosToken> p0)
    {
        return default(object);
    }

    [Operand]
    [Production("literal_list : PARENS_LEFT literal (COMMA literal) * PARENS_RIGHT")]
    public object literallist_PARENSLEFT_literal_COMMA_literal_PARENSRIGHT(Token<SlowOnBadParseEosToken> p0, object p1, List<Group<SlowOnBadParseEosToken, object>> p2, Token<SlowOnBadParseEosToken> p3)
    {
        return default(object);
    }

    [Operand]
    [Production("function_call : IDENTIFIER PARENS_LEFT operand_list PARENS_RIGHT")]
    public object functioncall_IDENTIFIER_PARENSLEFT_operandlist_PARENSRIGHT(Token<SlowOnBadParseEosToken> p0, Token<SlowOnBadParseEosToken> p1, object p2, Token<SlowOnBadParseEosToken> p3)
    {
        return default(object);
    }

    [Production("operand_list : SlowOnBadParseEos_expressions (COMMA SlowOnBadParseEos_expressions) *")]
    public object operandlist_SlowOnBadParseEosexpressions_COMMA_SlowOnBadParseEosexpressions_(object p0, List<Group<SlowOnBadParseEosToken, object>> p1)
    {
        return default(object);
    }
}