using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using NFluent;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{

    [Lexer(KeyWordIgnoreCase = true)]
    public enum ContextualToken
    {
        A,
        B,
        C
    }
    
    public class ErrorTests
    {
        [Fact]
        public void TestExpressionSyntaxError()
        {
            var exprParser = new ExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, int>("en");
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
            Check.That(error.Line).IsEqualTo(0);
            Check.That(error.Column).IsEqualTo(10);
            Check.That(error.ErrorMessage).Contains("unexpected plus sign ('+ (line 0, column 10)'). Expecting INT, opening parenthesis, minus sign, .");
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
            Check.That(error.Line).IsEqualTo(2);
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
            Check.That(error.Line).IsEqualTo(0);
            Check.That(error.Column).IsEqualTo(2);
        }

        [Fact]
        public void TestLexicalError()
        {
            var exprParser = new ExpressionParser();

            var builder = new ParserBuilder<ExpressionToken, int>("en");
            var Parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;
            var r = Parser.Parse("2 @ 2");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Errors).IsNotNull();
            Check.That(r.Errors).CountIs(1);
            var error = r.Errors[0] as LexicalError;
            Check.That(error.Line).IsEqualTo(0);
            Check.That(error.Column).IsEqualTo(3);
            Check.That(error.UnexpectedChar).IsEqualTo('@');
            Check.That(error.ErrorMessage).IsEqualTo("Lexical Error, line 0, column 3 : Unrecognized symbol '@' (64)");
        }


        [Fact]
        public void TestContextualError()
        {
            var lexer = FluentLexerBuilder<ContextualToken>.NewBuilder()
                .IgnoreEol(true)
                .IgnoreWhiteSpace(true)
                .IgnoreKeywordCase(true)
                .Keyword(ContextualToken.A, "a")
                .Keyword(ContextualToken.B, "b")
                .Keyword(ContextualToken.C, "c");

            var build = FluentEBNFParserBuilder<ContextualToken, string>.NewBuilder(new FluentTests(), "root", "en")
                .Production("root : A B C", (objects => "ok"))
                .WithLexerbuilder(lexer)
                .BuildParser();

            Check.That(build).IsOk();

            var source = "a b c";
            var parsed = build.Result.Parse(source);
            Check.That(parsed).IsOkParsing();
            
            source = @"
a c b";
            parsed = build.Result.Parse(source);
            Check.That(parsed).Not.IsOkParsing();
            Check.That(parsed.Errors).CountIs(1);
            var error = parsed.Errors[0];
            var message = error.ContextualErrorMessage;
            var lines = message.GetLines();
            Check.That(lines).CountIs(4);
            Check.That(lines[2]).Contains("1 |a c b");
            Check.That(lines[3]).Contains("  |  ^^^ expected B");
            
            source = "a , c b";
            parsed = build.Result.Parse(source);
            Check.That(parsed).Not.IsOkParsing();
            Check.That(parsed.Errors).CountIs(1);
            error = parsed.Errors[0];
            message = error.ContextualErrorMessage;
            lines = message.GetLines();
            Check.That(lines).CountIs(4);
            Check.That(lines[2]).Contains("0 |a , c b");
            Check.That(lines[3]).Contains("  |  ^^^ unexpected char ','");



        }
    }
}