using System;
using simpleExpressionParser;
using sly.parser.generator;

namespace ParserExample;

public class Stacker
{
    
    public static void Stack()
    {
        var instance = new EvenSimplerStackParser();
        ParserBuilder<SimplerStackLexer, string> builder = new ParserBuilder<SimplerStackLexer, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK,"root");
        if (parser.IsOk)
        {
            parser.Result.Parse("1 2");
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
    
    public static void MoreStack()
    {
        var instance = new SimplerStackParser();
        ParserBuilder<SimplerStackLexer, string> builder = new ParserBuilder<SimplerStackLexer, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK,"root");
        if (parser.IsOk)
        {
            parser.Result.Parse("1 2 3");
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
    
    public static void EvenMoreStack()
    {
        var instance = new SimpleStackParser();
        ParserBuilder<SimpleStackLexer, int> builder = new ParserBuilder<SimpleStackLexer, int>();
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