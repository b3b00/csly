using sly.parser;
using expressionparser;
using jsonparser;
using sly.lexer;
using sly.parser.generator;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ParserTests
{
    public class ErrorTests
    {


        [Fact]
        public void TestJsonSyntaxError()
        {
            JSONParser jsonParser = new JSONParser();
            Parser<JsonToken> parser = ParserBuilder.BuildParser<JsonToken>(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");

            string source = @"{
                'one': 1,
                'bug':{,}
            }";
            ParseResult<JsonToken> r = parser.Parse(source);
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(UnexpectedTokenSyntaxError<JsonToken>), r.Errors[0]);
            UnexpectedTokenSyntaxError<JsonToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Assert.Equal(JsonToken.COMMA, error?.UnexpectedToken.TokenID);
            Assert.Equal(3, error?.Line);
            Assert.Equal(25, error?.Column);

        }

        [Fact]
        public void TestExpressionSyntaxError()
        {

            Parser<ExpressionToken> Parser = ParserBuilder.BuildParser<ExpressionToken>(typeof(ExpressionParser), ParserType.LL_RECURSIVE_DESCENT, "expression");

            ParseResult<ExpressionToken> r = Parser.Parse(" 2 + 3 + + 2");
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(UnexpectedTokenSyntaxError<ExpressionToken>), r.Errors[0]);
            UnexpectedTokenSyntaxError<ExpressionToken> error = r.Errors[0] as UnexpectedTokenSyntaxError<ExpressionToken>;

            Assert.Equal(ExpressionToken.PLUS, error.UnexpectedToken.TokenID);

            Assert.Equal(1, error.Line);
            Assert.Equal(9, error.Column);
        }

        [Fact]
        public void TestLexicalError()
        {

            Parser<ExpressionToken> Parser = ParserBuilder.BuildParser<ExpressionToken>(typeof(ExpressionParser), ParserType.LL_RECURSIVE_DESCENT, "root");
            ParseResult<ExpressionToken> r = Parser.Parse("2 @ 2");
            Assert.True(r.IsError);
            Assert.Null(r.Result);
            Assert.NotNull(r.Errors);
            Assert.True(r.Errors.Count > 0);
            Assert.IsAssignableFrom(typeof(LexicalError), r.Errors[0]);
            LexicalError error = r.Errors[0] as LexicalError;
            Assert.Equal(1, error.Line);
            Assert.Equal(2, error.Column);
            Assert.Equal('@', error.UnexpectedChar);
        }
    }
}
