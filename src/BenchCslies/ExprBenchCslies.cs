using BenchCslies.parsers.expressions.csly;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using generatedExpressions;
using generatedExpressions.generatedgenericsimpleexpressionparser;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.generator;

namespace BenchCslies;

[Config(typeof(ExprBenchCslies.EConfig))]
[MemoryDiagnoser]
public class ExprBenchCslies
{
    private class EConfig : ManualConfig
    {
        public EConfig()
        {
            var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
        }
    }
    
    private Parser<GenericExpressionToken, double> _cslyParser;
    
    private Parser<GenericExpressionToken, double> _fluentParser;

    private GeneratedGenericSimpleExpressionParserMain _generatedParser;

    private string _expression;

    public void Setup()
    {
        ParserBuilder<GenericExpressionToken,double> builder = new ParserBuilder<GenericExpressionToken,double>();
        var instance = new GenericSimpleExpressionParser();
        var r = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        if (r.IsError)
        {
            foreach (var error in r.Errors)
            {
                Console.Error.WriteLine(error);
            }
            Environment.Exit(1);
        }
        _cslyParser = r.Result;

        var fluentLexerBuilder = FluentLexerBuilder<GenericExpressionToken>.NewBuilder()
            .Double(GenericExpressionToken.DOUBLE)
            .Int(GenericExpressionToken.INT)
            .AlphaNumId(GenericExpressionToken.IDENTIFIER)
            .Sugar(GenericExpressionToken.PLUS, "+")
            .Sugar(GenericExpressionToken.MINUS, "-")
            .Sugar(GenericExpressionToken.TIMES, "*")
            .Sugar(GenericExpressionToken.DIVIDE, "/")
            .Sugar(GenericExpressionToken.LPAREN, "(")
            .Sugar(GenericExpressionToken.RPAREN, ")")
            .Sugar(GenericExpressionToken.FACTORIAL, "!");
        
        var fluentInstance =  new GenericSimpleExpressionParser();
        var fluentParserBuild = FluentEBNFParserBuilder<GenericExpressionToken,double>.NewBuilder(fluentInstance,"root")
            .Production("root : GenericSimpleExpressionParser_expressions", (args) =>
            {
                return (double)args[0];
            })
            .Left(GenericExpressionToken.MINUS,10, (args) =>
            {
                var left = (double)args[0];
                var right = (double)args[2];
                return left - right;
            } )
            .Left(GenericExpressionToken.PLUS,10, (args) =>
            {
                var left = (double)args[0];
                var right = (double)args[2];
                return left + right;
            } )
            .Left(GenericExpressionToken.TIMES,50, (args) =>
            {
                var left = (double)args[0];
                var right = (double)args[2];
                return left * right;
            } )
            .Left(GenericExpressionToken.DIVIDE,50, (args) =>
            {
                var left = (double)args[0];
                var right = (double)args[2];
                return left / right;
            } )
            .Prefix(GenericExpressionToken.MINUS,100, (args) =>
            {
                var value = (double)args[1];
                return -value;
            })
            .Postfix(GenericExpressionToken.FACTORIAL,100, (args) =>
            {
                var value = (double)args[0];
                var factorial = 1;
                for (var i = 1; i <= value; i++) factorial = factorial * i;
                return factorial;
            })
            .Operand("operand : primary_value", (args) =>
            {
                return (double)args[0];
            })
            .Production("primary_value: DOUBLE", (args) =>
            {
                var token = (Token<GenericExpressionToken>)args[0];
                return token.DoubleValue;
            })
            .Production("primary_value: INT", (args) =>
            {
                var token = (Token<GenericExpressionToken>)args[0];
                return token.DoubleValue;
            })
            .Production("primary_value : LPAREN[d] GenericSimpleExpressionParser_expressions RPAREN[d]", (args) =>
            {
                return (double)args[0];
            })
            .WithLexerbuilder(fluentLexerBuilder)
            .BuildParser();
        if (fluentParserBuild.IsError)
        {
            foreach (var error in fluentParserBuild.Errors)
            {
                Console.Error.WriteLine(error.Message);
            }
            Environment.Exit(1);
        }

        _fluentParser = fluentParserBuild.Result;

        var generatedParserInstance = new GeneratedGenericSimpleExpressionParser();
        _generatedParser = new GeneratedGenericSimpleExpressionParserMain(generatedParserInstance);

        _expression = "1"+("+1".Multiply(500));


    }

    [Benchmark]
    public void TestCsly()
    {
        try
        {
            var r = _cslyParser.Parse(_expression);
            if (r.IsOk)
            {
                Console.WriteLine("parse ok " + r.Result);
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.Error.WriteLine(error.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText("c:/tmp/error.txt", ex.Message+"\n"+ex.StackTrace);
        }
    }

    [Benchmark]
    public void TestFluent()
    {
        try
        {
            var r = _fluentParser.Parse(_expression);
            if (r.IsOk)
            {
                Console.WriteLine("parse ok " + r.Result);
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.Error.WriteLine(error.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText("c:/tmp/error.txt", ex.Message+"\n"+ex.StackTrace);
        }
    }

    [Benchmark]
    public void TestGenerated()
    {
        try
        {
            Console.WriteLine("generated");
            var r = _generatedParser.Parse(_expression);
            if (r.IsOk)
                Console.WriteLine("parse ok " + r.Result);
            else
            {
                Console.WriteLine("parse fail (generated)");
                foreach (var error in r.Errors)
                {
                        Console.WriteLine(error.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText("c:/tmp/error.txt", ex.Message+"\n"+ex.StackTrace);
        }
    }
}