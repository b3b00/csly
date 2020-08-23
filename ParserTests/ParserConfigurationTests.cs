using System.Collections.Generic;
using System.Linq;
using expressionparser;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public enum BadTokens
    {
        [Lexeme("a++")] BadRegex = 1,
        [Lexeme("b")] Good = 2,

        MissingLexeme = 3
    }

    public enum Tokens
    {
        [Lexeme("a")] A = 1,
        [Lexeme("b")] B = 2,
        [Lexeme("c")] C = 3,
    }
    
    public class BadReturnParser {

        [Production("badreturn : A B")]
        public string BadReturn(Token<Tokens> a, Token<Tokens> b)
        {
            return "toto";
        }
    }

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
        public void TestGrammarBuildErrors()
        {
            var parserBuilder = new ParserBuilder<ExpressionToken, int>();
            var instance = new ParserConfigurationTests();
            var result = parserBuilder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "R");
            Assert.True(result.IsError);
            Assert.Equal(2, result.Errors.Count);
            var warnerrors = result.Errors.Where(e => e.Level == ErrorLevel.WARN).ToList();
            var errorerrors = result.Errors.Where(e => e.Level == ErrorLevel.ERROR).ToList();
            Assert.Single(warnerrors);
            Assert.True(warnerrors[0].Message.Contains("R3") && warnerrors[0].Message.Contains("never used"));
            Assert.Single(errorerrors);
            Assert.True(errorerrors[0].Message.Contains("R2") && errorerrors[0].Message.Contains("not exist"));
        }

        [Fact]
        public void TestLexerBuildErrors()
        {
            var result = new BuildResult<ILexer<BadTokens>>();
            result = LexerBuilder.BuildLexer(result);

            Assert.True(result.IsError);
            Assert.Equal(2, result.Errors.Count);
            var errors = result.Errors.Where(e => e.Level == ErrorLevel.ERROR).ToList();
            var warnings = result.Errors.Where(e => e.Level == ErrorLevel.WARN).ToList();
            Assert.Single(errors);
            var errorMessage = errors[0].Message;
            Assert.True(errorMessage.Contains(BadTokens.BadRegex.ToString()) && errorMessage.Contains("BadRegex"));
            Assert.Single(warnings);
            var warnMessage = warnings[0].Message;
            Assert.True(warnMessage.Contains(BadTokens.MissingLexeme.ToString()) &&
                        warnMessage.Contains("not have Lexeme"));
        }

        [Fact]
        public void TestBadVisitorReturn()
        {
            var instance = new BadReturnParser();
            ParserBuilder<Tokens,List<string>> builder = new ParserBuilder<Tokens, List<string>>();
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badreturn");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            Assert.Contains("incorrect return type", result.Errors.First().Message);
            Assert.Contains("visitor BadReturn", result.Errors.First().Message);
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badreturn");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            Assert.Contains("incorrect return type", result.Errors.First().Message);
            Assert.Contains("visitor BadReturn", result.Errors.First().Message);
            ;
        }
    }
}