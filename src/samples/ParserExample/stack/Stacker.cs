using System;
using expressionparser;
using NFluent;
using ParserTests;
using simpleExpressionParser;
using sly.parser.generator;
using ExpressionToken = expressionparser.ExpressionToken;

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
            var r = parser.Result.Parse("1 2");
            if (r.IsOk)
            {
                Console.WriteLine($"PARSE OK !!! >{r.Result}<");
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
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
            var r = parser.Result.Parse("1");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo("1");
            r = parser.Result.Parse("1 2");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo("1,2");
            r = parser.Result.Parse("1 2 3 4 5");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo("1,2,3,4,5");
            
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
            var r = parser.Result.Parse("1+2+3+4");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(10);
            r = parser.Result.Parse("1+2+3+4+5+6+7+8+9+10");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(55);
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
        
        var instance2 = new ExpressionParser();
        ParserBuilder<ExpressionToken, int> builder2 = new ParserBuilder<ExpressionToken, int>();
        var parser2 = builder2.BuildParser(instance2, ParserType.LL_STACK,"expression");
        if (parser.IsOk)
        {
            var r = parser.Result.Parse("1+2+3+4");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(10);
            r = parser.Result.Parse("1+2+3+4+5+6+7+8+9*10");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(135);
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