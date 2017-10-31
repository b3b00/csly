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
    public class ParserConfigurationTests
    {

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

        [Fact]
        public void TestBuildErrors()
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
        


    }
}
