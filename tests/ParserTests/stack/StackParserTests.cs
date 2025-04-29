using NFluent;
using simpleExpressionParser;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser.fluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.stack;

public class Dumb
{
    
}

public class StackParserTests
{
    [Fact]
    public void basic()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .Int(ExpressionToken.INT);
        var parser = FluentParserBuilder<ExpressionToken, string>.NewBuilder(new Dumb(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : expr", (object[] args) => (string)args[0])
            .Production("expr : INT", (args) => ((Token<ExpressionToken>)args[0]).Value)
            .Production("expr : INT expr", (args) => ((Token<ExpressionToken>)args[0]).Value+ "," + (string)args[1])
            .BuildParser(ParserType.LL_STACK);
        Check.That(parser).IsOk();
        var result = parser.Result.Parse("1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1");
        result = parser.Result.Parse("1 2 3 4 5");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1,2,3,4,5");
        


    }
}