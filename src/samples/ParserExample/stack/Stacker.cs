using System;
using expressionparser;
using NFluent;
using ParserTests;
using simpleExpressionParser;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser.fluent;
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

    public static void FluentStack()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .Int(ExpressionToken.INT);
        var parser = FluentParserBuilder<ExpressionToken, string>.NewBuilder(new ParserTests.stack.SimplerStackParser(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : expr", (object[] args) =>
            {
                return (string)args[0];
            })
            .Production("expr : INT", (args) =>
            {
                return ((Token<ExpressionToken>)args[0]).Value;
            })
            .Production("expr : INT expr", (args) =>
            {
                return ((Token<ExpressionToken>)args[0]).Value + "," + (string)args[1];
            })
            .BuildParser(ParserType.LL_STACK);
        Check.That(parser).IsOk();
        var result = parser.Result.Parse("1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1");
        result = parser.Result.Parse("1 2 3 4 5");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1,2,3,4,5");
    }

    public static void EvenMoreStack()
    {
        var instance = new SimpleStackParser();
        ParserBuilder<SimpleStackLexer, int> builder = new ParserBuilder<SimpleStackLexer, int>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
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
    }
    public static void Expression()
    {
    var instance = new ExpressionParser();
        ParserBuilder<expressionparser.ExpressionToken, int> builder2 = new ParserBuilder<expressionparser.ExpressionToken, int>();
        var parser = builder2.BuildParser(instance, ParserType.LL_STACK,"expression");
        if (parser.IsOk)
        {
            string source = "2+2";
;            Console.WriteLine($"start parsing {source}");
            var r = parser.Result.Parse(source);
            Console.WriteLine($"parsing done : {(r.IsOk ? "OK": "KO")}");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(4);
            r = parser.Result.Parse("1+2+3+4+5+6+7+8+9*10");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(126);
            Console.WriteLine("parsing done !!! OOH YEAH !! ");;
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