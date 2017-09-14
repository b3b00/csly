using expressionparser;
using jsonparser;
using NUnit.Framework;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace ParserTests
{
    [TestFixture]
    public class ErrorTests
    {
        [Test]
        public void TestJsonSyntaxError()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<JsonToken> parser = builder.BuildParser<JsonToken>(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");

            string source = @"{
                'one': 1,
                'bug':{,}
            }".Replace("'", "\"");
            ParseResult<JsonToken> r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(UnexpectedTokenSyntaxError<JsonToken>), r.Errors[0]);
            UnexpectedTokenSyntaxError<JsonToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.AreEqual(JsonToken.COMMA, error?.UnexpectedToken.TokenID);
            Assert.AreEqual(3, error?.Line);
            Assert.AreEqual(24, error?.Column);

        }

        [Test]
        public void TestExpressionSyntaxError()
        {
            ExpressionParser exprParser = new ExpressionParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<ExpressionToken> Parser = builder.BuildParser<ExpressionToken>(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression");

            ParseResult<ExpressionToken> r = Parser.Parse(" 2 + 3 + + 2");
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(UnexpectedTokenSyntaxError<ExpressionToken>), r.Errors[0]);
            UnexpectedTokenSyntaxError<ExpressionToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<ExpressionToken>;

            Assert.AreEqual(ExpressionToken.PLUS, error.UnexpectedToken.TokenID);

            Assert.AreEqual(1, error.Line);
            Assert.AreEqual(10, error.Column);
        }

        [Test]
        public void TestLexicalError()
        {
            ExpressionParser exprParser = new ExpressionParser();

            ParserBuilder builder = new ParserBuilder();
            Parser<ExpressionToken> Parser = builder.BuildParser<ExpressionToken>(exprParser, ParserType.LL_RECURSIVE_DESCENT, "root");
            ParseResult<ExpressionToken> r = Parser.Parse("2 @ 2");
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(LexicalError), r.Errors[0]);
            LexicalError error = r.Errors[0] as LexicalError;
            Assert.AreEqual(1, error.Line);
            Assert.AreEqual(3, error.Column);
            Assert.AreEqual('@', error.UnexpectedChar);
        }
    }
}
