using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using NFluent;
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
            Check.That(r.IsError).IsTrue();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            var err = r.Errors[0];
            Check.That(err).IsNotNull();
            Check.That(err).IsInstanceOf<UnexpectedTokenSyntaxError<ExpressionToken>>();
            var error = err as UnexpectedTokenSyntaxError<ExpressionToken>;
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo(ExpressionToken.PLUS);
            Check.That(error.Line).IsEqualTo(1);
            Check.That(error.Column).IsEqualTo(10);
        }

        [Fact]
        public void TestJsonEbnfSyntaxMissingLastClosingBracket()
        {
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var Parser = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root").Result;

            var source = "{";

            var r = Parser.Parse(source);
            Check.That(r.IsError).IsTrue();
            Check.That(r.Result).IsNull();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            Check.That(r.Errors[0]).IsInstanceOf<UnexpectedTokenSyntaxError<JsonTokenGeneric>>();
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonTokenGeneric>;

            Check.That(error).IsNotNull();
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo((JsonTokenGeneric)0);
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
            Check.That(error.Line).IsEqualTo(0);
            Check.That(error.Column).IsEqualTo(1);
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
            
            Check.That(r.IsError).IsTrue();
            Check.That(r.Result).IsNull();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            Check.That(r.Errors[0]).IsInstanceOf<UnexpectedTokenSyntaxError<JsonToken>>();
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo(JsonToken.COMMA);
            Check.That(error.Line).IsEqualTo(3);
            Check.That(error.Column).IsEqualTo(12);
        }

        [Fact]
        public void TestJsonSyntaxErrorMissingLastClosingBracket()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;

            var source = "{";

            var r = parser.Parse(source);
            Check.That(r.IsError).IsTrue();
            Check.That(r.Result).IsNull();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            Check.That(r.Errors[0]).IsInstanceOf<UnexpectedTokenSyntaxError<JsonToken>>();
            var error = r.Errors[0] as UnexpectedTokenSyntaxError<JsonToken>;

            Check.That(error).IsNotNull();
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo((JsonToken) 0);
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
            Check.That(error.Line).IsEqualTo(1);
            Check.That(error.Column).IsEqualTo(2);
        }

        [Fact]
        public void TestLexicalError()
        {
            var exprParser = new ExpressionParser();

            var builder = new ParserBuilder<ExpressionToken, int>();
            var Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;
            var r = Parser.Parse("2 @ 2");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            var error = r.Errors[0] as LexicalError;
            Check.That(error.Line).IsEqualTo(1);
            Check.That(error.Column).IsEqualTo(3);
            Check.That(error.UnexpectedChar).IsEqualTo('@');
        }
    }
}