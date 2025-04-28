using System;
using simpleExpressionParser;
using sly.parser.generator;

namespace ParserExample;

public class Stacker
{
    public static void Stack()
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