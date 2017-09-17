using sly.parser;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.parser.generator;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ParserTests
{
    public class LexerTests
    {

        private ILexer<JsonToken> GetJsonLexer()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<JsonToken,JSon> parser = builder.BuildParser<JsonToken,JSon>(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");
            return parser.Lexer;
        }

        private ILexer<ExpressionToken> GetExpressionLexer()
        {
            ExpressionParser exprParser = new ExpressionParser();
            ParserBuilder builder = new ParserBuilder();
            Parser<ExpressionToken,int> parser = builder.BuildParser<ExpressionToken,int>(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression");
            return parser.Lexer;
        }




        [Fact]
        public void TestSingleLineJsonLexing()
        {
            string json = "{ \"propi\": 12 , \"props\":\"val\" }";
            ILexer<JsonToken> lexer = GetJsonLexer();
            List<Token<JsonToken>> tokens = lexer.Tokenize(json).ToList<Token<JsonToken>>();
            Assert.Equal(10, tokens.Count);
            List<JsonToken> expectedTokensID = new List<JsonToken>()
            {
                JsonToken.ACCG, JsonToken.STRING,JsonToken.COLON,JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING,JsonToken.COLON,JsonToken.STRING,
                JsonToken.ACCD
            };
            List<JsonToken> tokensID = tokens.Take(9).Select((Token<JsonToken> tok) => tok.TokenID).ToList<JsonToken>();
            Assert.Equal(expectedTokensID, tokensID);

            List<int> expectedColumnPositions = new List<int>()
            {
                1,3,10,12,15,17,24,25,31
            };

            List<int> columnPositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Column).ToList<int>();
            Assert.Equal(expectedColumnPositions, columnPositions);

            ;
        }

        [Fact]
        public void TestSingleLineExpressionLexing()
        {
            ILexer<ExpressionToken> lexer = GetExpressionLexer();
        }

        [Fact]
        public void TestMultiLineJsonLexing()
        {
            string json = "{ \"propi\": 12 \n" +
             ", \"props\":\"val\" }";
            ILexer<JsonToken> lexer = GetJsonLexer();
            List<Token<JsonToken>> tokens = lexer.Tokenize(json).ToList<Token<JsonToken>>();
            Assert.Equal(10, tokens.Count);
            List<JsonToken> expectedTokensID = new List<JsonToken>()
            {
                JsonToken.ACCG, JsonToken.STRING,JsonToken.COLON,JsonToken.INT,
                JsonToken.COMMA, JsonToken.STRING,JsonToken.COLON,JsonToken.STRING,
                JsonToken.ACCD
            };
            List<JsonToken> tokensID = tokens.Take(9).Select((Token<JsonToken> tok) => tok.TokenID).ToList<JsonToken>();
            Assert.Equal(expectedTokensID, tokensID);

            List<int> expectedColumnPositions = new List<int>()
            {
                1,3,10,12,
                1,3,10,11,17
            };

            List<int> columnPositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Column).ToList<int>();
            Assert.Equal(expectedColumnPositions, columnPositions);

            List<int> expectedLinePositions = new List<int>()
            {
                1,1,1,1,2,2,2,2,2
            };

            List<int> linePositions = tokens.Take(9).Select((Token<JsonToken> tok) => tok.Position.Line).ToList<int>();
            Assert.Equal(expectedLinePositions, linePositions);

            ;
        }

        [Fact]
        public void TestMultiLineExpressionLexing()
        {
            ILexer<ExpressionToken> lexer = GetExpressionLexer();
        }
    }
}
