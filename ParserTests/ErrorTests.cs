using sly.parser;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public class ErrorTests
    {


        [Fact]
        public void TestJsonSyntaxError()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();
            Parser<JsonToken,JSon> parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;


            string source = @"{
    'one': 1,
    'bug':{,}
}".Replace("'", "\"");
            ParseResult<JsonToken,JSon> r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonToken>>(r.Errors[0]);
            UnexpectedTokenSyntaxError<JsonToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.Equal(JsonToken.COMMA, error?.UnexpectedToken.TokenID);
            Assert.Equal(2, error?.Line);
            Assert.Equal(13, error?.Column);

        }

        [Fact]
        public void TestExpressionSyntaxError()
        {
            ExpressionParser exprParser = new ExpressionParser();
            ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
            Parser<ExpressionToken,int> Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;

            ParseResult<ExpressionToken,int> r = Parser.Parse(" 2 + 3 + + 2");
            Assert.True(r.IsError);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom < UnexpectedTokenSyntaxError<ExpressionToken>>(r.Errors[0]);
            UnexpectedTokenSyntaxError<ExpressionToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<ExpressionToken>;

            Assert.Equal(ExpressionToken.PLUS, error.UnexpectedToken.TokenID);

            Assert.Equal(1, error.Line);
            Assert.Equal(8, error.Column);
        }

        [Fact]
        public void TestLexicalError()
        {
            ExpressionParser exprParser = new ExpressionParser();

            ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
            Parser<ExpressionToken,int> Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;
            ParseResult<ExpressionToken,int> r = Parser.Parse("2 @ 2");
            Assert.True(r.IsError);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<LexicalError>(r.Errors[0]);
            LexicalError error = r.Errors[0] as LexicalError;
            Assert.Equal(1, error.Line);
            Assert.Equal(3, error.Column);
            Assert.Equal('@', error.UnexpectedChar);
        }

        [Fact]
        public void TestJsonSyntaxErrorMissingLastClosingBracket()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();
            Parser<JsonToken, JSon> parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;

            string source = "{";

            ParseResult<JsonToken, JSon> r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonToken>>(r.Errors[0]);
            UnexpectedTokenSyntaxError<JsonToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.Equal((JsonToken)0, error?.UnexpectedToken.TokenID);
            Assert.True(error.ErrorMessage.Contains("end of stream"));
            Assert.Equal(1, error?.Line);
            Assert.Equal(2, error?.Column);
        }

        [Fact]
        public void TestJsonEbnfSyntaxMissingLastClosingBracket()
        {
            EbnfJsonGenericParser jsonParser = new EbnfJsonGenericParser();
            ParserBuilder<JsonTokenGeneric, JSon> builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            Parser<JsonTokenGeneric, JSon> Parser = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root").Result;

            string source = "{";

            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsType<UnexpectedTokenSyntaxError<JsonTokenGeneric>>(r.Errors[0]);
            UnexpectedTokenSyntaxError<JsonTokenGeneric> error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonTokenGeneric>;

            Assert.Equal((JsonTokenGeneric)0, error?.UnexpectedToken.TokenID);
             Assert.True(error.ErrorMessage.Contains("end of stream"));
            Assert.Equal(0, error?.Line);
            Assert.Equal(1, error?.Column);
        }

    }
}
