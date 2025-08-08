using System;
using NFluent;
using ParserTests;
using sly.i18n;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserExample;

public enum ContextualErrorToken
{
    [Int]
    [LexemeLabel("fr","entier")]
    [LexemeLabel("en","integer")]
    INT,
    
    [AlphaId]
    [LexemeLabel("fr","identifiant")]
    [LexemeLabel("en","identifier")]
    ID,
    
    [Sugar("+")]
    [LexemeLabel("fr","+")]
    [LexemeLabel("en","+")]
    PLUS,
    [Sugar("-")]
    [LexemeLabel("fr","-")]
    [LexemeLabel("en","-")]
    MINUS,
    [Sugar("*")]
    [LexemeLabel("fr","*")]
    [LexemeLabel("en","*")]
    MULTIPLY,
    [Sugar("/")]
    [LexemeLabel("fr","/")]
    [LexemeLabel("en","/")]
    DIVIDE,
    
    [Sugar("=")]
    [LexemeLabel("fr","=")]
    [LexemeLabel("en","=")]
    ASSIGN,
    
    [Sugar("==")]
    [LexemeLabel("fr","==")]
    [LexemeLabel("en","==")]
    EQUALS,
}

public class ContextualErrorParser
{
    [Production("root : 'if'[d] ContextualErrorParser_expressions 'then'[d] statement ('else'[d] statement)?'endif'[d]")]
    public string Root(string condition, string thenStatement, ValueOption<Group<ContextualErrorToken, string>> elseStatement)
    {
        var t = elseStatement.Match(
            els => $"else {els.Value(0)}",
            () => "none");
        return $"{condition} => {thenStatement} {t}";
    }
    
    [Operation((int) ContextualErrorToken.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
    public string BinaryTermExpression(string left, Token<ContextualErrorToken> operation, string right)
    {
        return $"( {left} {operation.Value} {right})";
    }

    [Production("statement : ID ASSIGN[d] ContextualErrorParser_expressions")]
    public string statement(Token<ContextualErrorToken> id, string value)
    {
        return $"{id.Value} <- {value}";
    }

    [Operation((int) ContextualErrorToken.MULTIPLY, Affix.InFix, Associativity.Right, 50)]
    [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
    public string BinaryFactorExpression(string left, Token<ContextualErrorToken> operation, string right)
    {
        return $"( {left} {operation.Value} {right})";
    }


    [Prefix((int) ContextualErrorToken.MINUS,  Associativity.Right, 100)]
    [NodeName("minus")]
    public string PreFixExpression(Token<ContextualErrorToken> operation, string value)
    {
        return $"- {value}";
    }
    
    [Operand]
    [Production("operand : INT")]
    public string OperandValue(Token<ContextualErrorToken> integer)
    {
        return integer.Value;
    }
    [Operand]
    [Production("operand : ID")]
    public string OperandId(Token<ContextualErrorToken> id)
    {
        return id.Value;
    }
    
    [Infix("EQUALS", Associativity.Right, 50)]
    public string Eq(string left , Token<ContextualErrorToken> operation, string right)
    {
        return $"{left} == {right}";
    }
}

public class ContextualErrorMessages
{
    
    public static Parser<ContextualErrorToken,string> BuildParser()
    {
        ParserBuilder<ContextualErrorToken, string> builder = new ParserBuilder<ContextualErrorToken, string>();
        var result = builder.BuildParser(new ContextualErrorParser(),ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        Check.That(result).IsOk();
        return result.Result;
    }

    public static void TestContextualErrors()
    {
        var parser = BuildParser();
        var source = @"
if 89 == 12 then
y = 14 +  28
else
y = 101 
@end";
        var result = parser.Parse(source);
        Check.That(result).Not.IsOkParsing();
        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error.ContextualErrorMessage);
        }
        
        source = @"
if 89 == 12 then
y = 14+ +  28
else
y = 101 
endif";
        result = parser.Parse(source);
        Check.That(result).Not.IsOkParsing();
        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error.ContextualErrorMessage);
        }
    }
    
}