using System;
using simpleExpressionParser;
using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

public enum SimpleLexer
{
    [Int] INT,
    [Sugar("+")] PLUS
}

[ParserRoot("root")]
public class SimpleParser
{
    [Production("root : expr")]
    public double root(double e) => e;
    
    [Production("expr : expr PLUS term")]
    public double expr(double e1, double e2) => e1 + e2;
    
    [Production("expr : term")]
    public double expr2(double e) => e;
    
    [Production("term : INT")]
    public double term(int i) => i;
}

public class Stacker
{
    public static void Stack()
    {
        var instance = new SimpleParser();
        ParserBuilder<SimpleLexer, double> builder = new ParserBuilder<SimpleLexer, double>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK,"root");
        if (parser.IsOk)
        {
            parser.Result.Parse("2+2");
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}