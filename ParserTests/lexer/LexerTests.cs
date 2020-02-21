using System.Collections.Generic;
using System.Linq;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
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
            var parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;
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
            Assert.NotNull(tokens);
            var tokenList = tokens.Tokens; 
            Assert.NotEmpty(tokenList);
            var token = tokenList[0];
            Assert.NotNull(token);
            Assert.Equal(1.68, token.DoubleValue);
        }
        
        [Fact]
        public void TestMultiLineExpressionLexing()
        {
            var lexer = GetExpressionLexer();
            var expr = "1 + 2 \n* 3";
            var r = lexer.Tokenize(expr);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(6, tokens.Count);
            var expectedTokensID = new List<ExpressionToken>
            {
                ExpressionToken.INT, ExpressionToken.PLUS, ExpressionToken.INT,
                ExpressionToken.TIMES, ExpressionToken.INT
            };
            var tokensID = tokens.Take(5).Select(tok => tok.TokenID).ToList();
            Assert.Equal(expectedTokensID, tokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 5, 1, 3
            };

            var columnPositions = tokens.Take(5).Select(tok => tok.Position.Column).ToList();
            Assert.Equal(expectedColumnPositions, columnPositions);

            var expectedLinePositions = new List<int>
            {
                1, 3, 5, 1, 3
            };

            var linePositions = tokens.Take(5).Select(tok => tok.Position.Line).ToList();
            Assert.Equal(expectedLinePositions, columnPositions);
        }

        [Fact]
        public void TestMultiLineJsonLexing()
        {
            var json = "{ \"propi\": 12 \n" +
                       ", \"props\":\"val\" }";
            var lexer = GetJsonLexer();
            var r = lexer.Tokenize(json);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(10, tokens.Count);
            var expectedTokensID = new List<JsonToken>
            {
                JsonToken.ACCG, JsonToken.STRING, JsonToken.COLON, JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING, JsonToken.COLON, JsonToken.STRING,
                JsonToken.ACCD
            };
            var tokensID = tokens.Take(9).Select(tok => tok.TokenID).ToList();
            Assert.Equal(expectedTokensID, tokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 10, 12,
                1, 3, 10, 11, 17
            };

            var columnPositions = tokens.Take(9).Select(tok => tok.Position.Column).ToList();
            Assert.Equal(expectedColumnPositions, columnPositions);

            var expectedLinePositions = new List<int>
            {
                1, 1, 1, 1, 2, 2, 2, 2, 2
            };

            var linePositions = tokens.Take(9).Select(tok => tok.Position.Line).ToList();
            Assert.Equal(expectedLinePositions, linePositions);
        }

        [Fact]
        public void TestSingleLineExpressionLexing()
        {
            var lexer = GetExpressionLexer();
            var expr = "1 + 2 * 3";
            var r = lexer.Tokenize(expr);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(6, tokens.Count);
            var expectedTokensID = new List<ExpressionToken>
            {
                ExpressionToken.INT, ExpressionToken.PLUS, ExpressionToken.INT,
                ExpressionToken.TIMES, ExpressionToken.INT
            };
            var tokensID = tokens.Take(5).Select(tok => tok.TokenID).ToList();
            Assert.Equal(expectedTokensID, tokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 5, 7, 9
            };

            var columnPositions = tokens.Take(5).Select(tok => tok.Position.Column).ToList();
            Assert.Equal(expectedColumnPositions, columnPositions);
        }


        [Fact]
        public void TestSingleLineJsonLexing()
        {
            var json = "{ \"propi\": 12 , \"props\":\"val\" }";
            var lexer = GetJsonLexer();
            var r = lexer.Tokenize(json);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(10, tokens.Count);
            var expectedTokensID = new List<JsonToken>
            {
                JsonToken.ACCG, JsonToken.STRING, JsonToken.COLON, JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING, JsonToken.COLON, JsonToken.STRING,
                JsonToken.ACCD
            };
            var tokensID = tokens.Take(9).Select(tok => tok.TokenID).ToList();
            Assert.Equal(expectedTokensID, tokensID);

            var expectedColumnPositions = new List<int>
            {
                1, 3, 10, 12, 15, 17, 24, 25, 31
            };

            var columnPositions = tokens.Take(9).Select(tok => tok.Position.Column).ToList();
            Assert.Equal(expectedColumnPositions, columnPositions);
        }
    }
}