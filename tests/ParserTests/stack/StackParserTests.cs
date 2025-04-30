using expressionparser;
using NFluent;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser.fluent;
using sly.parser.generator;
using Xunit;


namespace ParserTests.stack;

public class Dumb
{
    
}

[ParserRoot("root")]
public class SimplerStackParser
{
    [Production("root : expr")]
    public string root(string e) => e;

    [Production("expr : INT expr")]
    public string expr(Token<ExpressionToken> i, string e) => i.Value + "," + e;
    
    [Production("expr : INT")]
    public string expr2(Token<ExpressionToken> i) => i.Value;
    
}

public class StackParserTests
{

    [Fact]
    public void basic()
    {
        var instance = new SimplerStackParser();
        ParserBuilder<ExpressionToken, string> builder = new ParserBuilder<ExpressionToken, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
        Check.That(parser).IsOk();
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
    
    [Fact]
    public void expression()
    {
        var instance = new ExpressionParser();
        ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "expression");
        Check.That(parser).IsOk();
        var r = parser.Result.Parse("1");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(1);
        r = parser.Result.Parse("2 + 2");
        Check.That(r).IsOkParsing();
        //Check.That(r.Result).IsEqualTo(4);
        r = parser.Result.Parse("1 + 2 + 3 + 4 * 5");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(1+2+3+4*5);
    }

    [Fact]
    public void basicFluent()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .Int(ExpressionToken.INT);
        var parser = FluentParserBuilder<ExpressionToken, string>.NewBuilder(new SimplerStackParser(), "root", "en")
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
            .BuildParser(ParserType.LL_RECURSIVE_DESCENT);
        Check.That(parser).IsOk();
        var result = parser.Result.Parse("1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1");
        result = parser.Result.Parse("1 2 3 4 5");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1,2,3,4,5");
        


    }
}