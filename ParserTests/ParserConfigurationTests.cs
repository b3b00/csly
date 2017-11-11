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
using sly.buildresult;

namespace ParserTests
{

    public enum BadTokens
    {
        [Lexeme("a++")]
        BadRegex = 1,
        [Lexeme("b")]
        Good = 2,
        
        MissingLexeme = 3,
        
    }
    public class ParserConfigurationTests
    {

        #region test grammar

        [Production("R : R1 R2")]
        public int R(int r, int r2)
        {
            return r;
        }

        [Production("R1 : INT")]
        public int R1(Token<ExpressionToken> tok)
        {
            return tok.IntValue;
        }
        [Production("R3 : INT")]
        public int R3(Token<ExpressionToken> tok)
        {
            return tok.IntValue;
        }

        #endregion

        [Fact]
        public void TestGrammarBuildErrors()
        {
            ParserBuilder<ExpressionToken, int> parserBuilder = new ParserBuilder<ExpressionToken, int>();
            ParserConfigurationTests instance = new ParserConfigurationTests();
            var result = parserBuilder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "R");
            Assert.True(result.IsError);
            Assert.Equal(2, result.Errors.Count);
            var warnerrors = result.Errors.Where(e => e.Level == sly.buildresult.ErrorLevel.WARN).ToList();
            var errorerrors = result.Errors.Where(e => e.Level == sly.buildresult.ErrorLevel.ERROR).ToList();
            Assert.Equal(1, warnerrors.Count);
            Assert.True(warnerrors[0].Message.Contains("R3") && warnerrors[0].Message.Contains("never used"));
            Assert.Equal(1, errorerrors.Count);
            Assert.True(errorerrors[0].Message.Contains("R2") && errorerrors[0].Message.Contains("not exist"));
        }

        [Fact]
        public void TestLexerBuildErrors()
        {
            var result = new BuildResult<ILexer<BadTokens>>();
            result = LexerBuilder.BuildLexer<BadTokens>(result);

            Assert.True(result.IsError);
            Assert.Equal(2, result.Errors.Count);
            var errors = result.Errors.Where(e => e.Level == ErrorLevel.ERROR).ToList();
            var warnings = result.Errors.Where(e => e.Level == ErrorLevel.WARN).ToList();
            Assert.Equal(1, errors.Count);
            var errorMessage = errors[0].Message;
            Assert.True(errorMessage.Contains(BadTokens.BadRegex.ToString()) && errorMessage.Contains("BadRegex"));
            Assert.Equal(1, warnings.Count);
            var warnMessage = warnings[0].Message;
            Assert.True(warnMessage.Contains(BadTokens.MissingLexeme.ToString()) && warnMessage.Contains("not have Lexeme"));
            ;


        }
    }
}
