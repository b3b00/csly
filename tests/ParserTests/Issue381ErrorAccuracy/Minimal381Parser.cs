using System.Collections.Generic;
using ParserTests.errorAccuracyIssue381;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue381ErrorAccuracy;

public class Minimal381Parser
{
    [Production("statements : statement*")]
    public object Statements(List<object> statements) => null;

    [Production($"statement : Id Set[d] {nameof(Minimal381Parser)}_expressions")]
    public object Set(Token<ErrorAccuracyIssue381Token> id, object functionCall) => null;
    
    [Operand]
    [Production("functionCall : Id Lparen[d] args Rparen[d]")]
    public object FunctionCall(Token<ErrorAccuracyIssue381Token> d, object args) => null;

    [Production($"args : {nameof(Minimal381Parser)}_expressions (Comma[d] {nameof(Minimal381Parser)}_expressions)*")]
    public object Arguments(object head, List<Group<ErrorAccuracyIssue381Token, object>> tail) => null;

    [Production("operand: [String | Id]")]
    public object Operand(Token<ErrorAccuracyIssue381Token> operand) => null;
    
    
    [Operation("Plus", Affix.InFix, Associativity.Right, 10)]
    // using token string representation
    [Operation("Minus", Affix.InFix, Associativity.Left, 10)]
    public object BinaryTermExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
    {
        return null;
    }


    [Operation("Times", Affix.InFix, Associativity.Right, 50)]
    [Operation("Div", Affix.InFix, Associativity.Left, 50)]
    public object BinaryFactorExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
    {
        return null;
    }

    [Operation("Minus", Affix.PreFix, Associativity.Left, 55)]
    public object MinusExpression(Token<ErrorAccuracyIssue381Token> minus, object expression)
    {
        return null;
    }
    
}