using System.Collections.Generic;
using System.Linq;
using expressionparser;
using jsonparser;
using NUnit.Framework;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace ParserTests
{
    [TestFixture]
    public class LexerTests
    {

        private ILexer<JsonToken> GetJsonLexer()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<JsonToken> parser = builder.BuildParser<JsonToken>(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");
            return parser.Lexer;
        }

        private ILexer<ExpressionToken> GetExpressionLexer()
        {
            ExpressionParser exprParser = new ExpressionParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<ExpressionToken> parser = builder.BuildParser<ExpressionToken>(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression");
            return parser.Lexer;
        }

        [Test]
        public void TestSingleLineJsonLexing()
        {
            string json = "{ \"propi\": 12 , \"props\":\"val\" }";
            ILexer<JsonToken> lexer = GetJsonLexer();
            List<Token<JsonToken>> tokens = lexer.Tokenize(json).ToList<Token<JsonToken>>();
            Assert.AreEqual(10, tokens.Count);
            List<JsonToken> expectedTokensID = new List<JsonToken>()
            {
                JsonToken.ACCG, JsonToken.STRING,JsonToken.COLON,JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING,JsonToken.COLON,JsonToken.STRING,
                JsonToken.ACCD
            };
            List<JsonToken> tokensID = tokens.Take(9).Select((Token<JsonToken> tok) => tok.TokenID).ToList<JsonToken>();
            Assert.AreEqual(expectedTokensID, tokensID);

            List<int> expectedColumnPositions = new List<int>()
            {
                1,3,10,12,15,17,24,25,31
            };

            List<int> columnPositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Column).ToList<int>();
            Assert.AreEqual(expectedColumnPositions, columnPositions);
        }

        [Test]
        public void TestSingleLineExpressionLexing()
        {
            ILexer<ExpressionToken> lexer = GetExpressionLexer();
        }

        [Test]
        public void TestMultiLineJsonLexing()
        {
            string json = "{ \"propi\": 12 \n" +
             ", \"props\":\"val\" }";
            ILexer<JsonToken> lexer = GetJsonLexer();
            List<Token<JsonToken>> tokens = lexer.Tokenize(json).ToList<Token<JsonToken>>();
            Assert.AreEqual(10, tokens.Count);
            List<JsonToken> expectedTokensID = new List<JsonToken>()
            {
                JsonToken.ACCG, JsonToken.STRING,JsonToken.COLON,JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING,JsonToken.COLON,JsonToken.STRING,
                JsonToken.ACCD
            };
            List<JsonToken> tokensID = tokens.Take(9).Select((Token<JsonToken> tok) => tok.TokenID).ToList<JsonToken>();
            Assert.AreEqual(expectedTokensID, tokensID);

            List<int> expectedColumnPositions = new List<int>()
            {
                1,3,10,12,
                1,3,10,11,17
            };

            List<int> columnPositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Column).ToList<int>();
            Assert.AreEqual(expectedColumnPositions, columnPositions);

            List<int> expectedLinePositions = new List<int>()
            {
                1,1,1,1,2,2,2,2,2
            };

            List<int> linePositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Line).ToList<int>();
            Assert.AreEqual(expectedLinePositions, linePositions);
        }

        [Test]
        public void TestMultiLineExpressionLexing()
        {
            ILexer<ExpressionToken> lexer = GetExpressionLexer();
        }
    }
}
