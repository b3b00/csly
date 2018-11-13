using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public class ErrorTests
    {
        [Fact]
        public void TestExpressionSyntaxError()
        {
            var exprParser = new ExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;

            var r = Parser.Parse(" 2 + 3 + + 2");
            Assert.True(r.IsError);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom<UnexpectedTokenSyntaxError<ExpressionToken>>(r.Errors[0]);
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<ExpressionToken>;

            Assert.Equal(ExpressionToken.PLUS, error.UnexpectedToken.TokenID);

            Assert.Equal(1, error.Line);
            Assert.Equal(8, error.Column);
        }

        [Fact]
        public void TestJsonEbnfSyntaxMissingLastClosingBracket()
        {
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var Parser = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root").Result;

            var source = "{";

            var r = Parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonTokenGeneric>>(r.Errors[0]);
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonTokenGeneric>;

            Assert.NotNull(error);
            Assert.Equal((JsonTokenGeneric) 0, error?.UnexpectedToken.TokenID);
            Assert.Contains("end of stream", error.ErrorMessage);
            Assert.Equal(0, error?.Line);
            Assert.Equal(1, error?.Column);
        }


        [Fact]
        public void TestJsonSyntaxError()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;


            var source = @"{
    'one': 1,
    'bug':{,}
}".Replace("'", "\"");
            var r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonToken>>(r.Errors[0]);
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.Equal(JsonToken.COMMA, error?.UnexpectedToken.TokenID);
            Assert.Equal(2, error?.Line);
            Assert.Equal(13, error?.Column);
        }

        [Fact]
        public void TestJsonSyntaxErrorMissingLastClosingBracket()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;

            var source = "{";

            var r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonToken>>(r.Errors[0]);
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.NotNull(error);
            Assert.Equal((JsonToken) 0, error?.UnexpectedToken.TokenID);
            Assert.Contains("end of stream", error.ErrorMessage);
            Assert.Equal(1, error?.Line);
            Assert.Equal(2, error?.Column);
        }

        [Fact]
        public void TestLexicalError()
        {
            var exprParser = new ExpressionParser();

            var builder = new ParserBuilder<ExpressionToken, int>();
            var Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;
            var r = Parser.Parse("2 @ 2");
            Assert.True(r.IsError);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<LexicalError>(r.Errors[0]);
            var error = r.Errors[0] as LexicalError;
            Assert.Equal(1, error.Line);
            Assert.Equal(3, error.Column);
            Assert.Equal('@', error.UnexpectedChar);
        }
    }
}