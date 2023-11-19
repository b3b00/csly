using System.Collections.Generic;
using System.Linq;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests.lexer
{
    public class LexerTests
    {
        private ILexer<JsonToken> GetJsonLexer()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var build = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");
            var parser = build.Result;
            return parser.Lexer;
        }

        private ILexer<ExpressionToken> GetExpressionLexer()
        {
            var exprParser = new ExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var parser = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
            return parser.Lexer;
        }

        [Fact]
        public void TestDoubleJsonLexing()
        {
            var lexer = GetJsonLexer();
            var tokens = lexer.Tokenize("1.68");
            Check.That(tokens).IsNotNull();
            var tokenList = tokens.Tokens; 
            Check.That(tokenList).Not.IsEmpty();
            var token = tokenList[0];
            Check.That(token).IsNotNull();
            Check.That(token.DoubleValue).IsEqualTo(1.68);
        }
        
        [Fact]
        public void TestMultiLineExpressionLexing()
        {
            var lexer = GetExpressionLexer();
            var expr = "1 + 2 \n* 3";
            var r = lexer.Tokenize(expr);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(6);
            var expectedTokensID = new List<ExpressionToken>
            {
                ExpressionToken.INT, ExpressionToken.PLUS, ExpressionToken.INT,
                ExpressionToken.TIMES, ExpressionToken.INT
            };
            Check.That(tokens.Take(5).Extracting(x => x.TokenID)).Contains(expectedTokensID);
            
            var expectedColumnPositions = new List<int>
            {
                1, 3, 5, 1, 3
            };

            Check.That(tokens.Take(5).Extracting(x => x.Position.Column)).Contains(expectedColumnPositions);
            

            var expectedLinePositions = new List<int>
            {
                1, 1,1,2,2
            };

            Check.That(tokens.Take(5).Extracting(x => x.Position.Line)).Contains(expectedLinePositions);
        }

        [Fact]
        public void TestMultiLineJsonLexing()
        {
            var json = "{ \"propi\": 12 \n" +
                       ", \"props\":\"val\" }";
            var lexer = GetJsonLexer();
            var r = lexer.Tokenize(json);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(10);
            var expectedTokensID = new List<JsonToken>
            {
                JsonToken.ACCG, JsonToken.STRING, JsonToken.COLON, JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING, JsonToken.COLON, JsonToken.STRING,
                JsonToken.ACCD
            };
            
            Check.That(tokens.Take(9).Extracting(x => x.TokenID)).Contains(expectedTokensID);
            
            
            var expectedColumnPositions = new List<int>
            {
                1, 3, 10, 12,
                1, 3, 10, 11, 17
            };

            Check.That(tokens.Take(9).Extracting(x => x.Position.Column)).Contains(expectedColumnPositions);

            var expectedLinePositions = new List<int>
            {
                1, 1, 1, 1, 2, 2, 2, 2, 2
            };

            Check.That(tokens.Take(9).Extracting(x => x.Position.Line)).Contains(expectedLinePositions);
        }

        [Fact]
        public void TestSingleLineExpressionLexing()
        {
            var lexer = GetExpressionLexer();
            var expr = "1 + 2 * 3";
            var r = lexer.Tokenize(expr);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(6);
            var expectedTokensID = new List<ExpressionToken>
            {
                ExpressionToken.INT, ExpressionToken.PLUS, ExpressionToken.INT,
                ExpressionToken.TIMES, ExpressionToken.INT
            };
            Check.That(tokens.Take(5).Extracting(x => x.TokenID)).Contains(expectedTokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 5, 7, 9
            };

            Check.That(tokens.Take(5).Extracting(x => x.Position.Column)).Contains(expectedColumnPositions);
        }


        [Fact]
        public void TestSingleLineJsonLexing()
        {
            var json = "{ \"propi\": 12 , \"props\":\"val\" }";
            var lexer = GetJsonLexer();
            var r = lexer.Tokenize(json);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(10);
            var expectedTokensID = new List<JsonToken>
            {
                JsonToken.ACCG, JsonToken.STRING, JsonToken.COLON, JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING, JsonToken.COLON, JsonToken.STRING,
                JsonToken.ACCD
            };
            Check.That(tokens.Take(9).Extracting(x => x.TokenID)).Contains(expectedTokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 10, 12, 15, 17, 24, 25, 31
            };

            Check.That(tokens.Take(9).Extracting(x => x.Position.Column)).Contains(expectedColumnPositions);
        }

        [Fact]
        public void TestMixedGenericRegexLexer()
        {
            var result = LexerBuilder.BuildLexer<MixedGenericRegexLexer>();
            Check.That(result.IsError).IsTrue();
            var errors = result.Errors;
            Check.That(errors).CountIs(1);
            Check.That(errors[0].Code).IsEqualTo(ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX);
        }
        
        
        [Fact]
        public void TestTokensAndError()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ExpressionToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var errorSource = "1 + 2 @";

            var lexingResult = lexer.Tokenize(errorSource);
            Check.That(lexingResult.IsError).IsTrue();
            Check.That(lexingResult.Tokens).IsNotNull();
            Check.That(lexingResult.Tokens).CountIs(3);
            Check.That(lexingResult.Error.UnexpectedChar).IsEqualTo('@');
        }
    }
}