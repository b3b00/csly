using System.Linq;
using expressionparser;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;
using Xunit;

namespace ParserTests
{
    public enum BadTokens
    {
        [Lexeme("a++")] BadRegex = 1,
        [Lexeme("b")] Good = 2,

        MissingLexeme = 3
    }

    public enum BadVisitorTokens
    {
        [Lexeme("a")] A = 1,
        [Lexeme("b")] B = 2,
        [Lexeme("c")] C = 3,
    }


    public interface BadVisitor
    {
        
    }

    public interface SubBadVisitor : BadVisitor
    {
        
    }
    
    public class BadVisitorReturnParser {

        [Production("badreturn : A B")]
        public string BadReturn(Token<BadVisitorTokens> a, Token<BadVisitorTokens> b)
        {
            return "toto";
        }
    }

    public class BadTerminalArgParser
    {
        [Production("badtermarg : A B")]
        public SubBadVisitor BarTermArg(string aArg, Token<BadVisitorTokens> bArg)
        {
            return null;
        }
    }
    
    public class BadNonTerminalArgParser
    {
        [Production("badnontermarg : a B")]
        public SubBadVisitor BadNonTermArg(string aArg, Token<BadVisitorTokens> bArg)
        {
            return null;
        }

        [Production("a : A")]
        public BadVisitor noTerm(Token<BadVisitorTokens> a)
        {
            return null;
        }
    }
    
    public class BadManyArgParser
    {
        [Production("badmanyarg : a* B* ( a B )+ [a|b]+")]
        public SubBadVisitor BadNonTermArg(string aArg, string bArg, string cArg, string dArg )
        {
            return null;
        }

        [Production("a : A")]
        public BadVisitor noTerm(Token<BadVisitorTokens> a)
        {
            return null;
        }
    }
    
    public class BadGroupArgParser
    {
        [Production("badgrouparg : ( a B )")]
        public SubBadVisitor BadNonTermArg(string aArg )
        {
            return null;
        }

        [Production("a : A")]
        public BadVisitor noTerm(Token<BadVisitorTokens> a)
        {
            return null;
        }
    }
    
    public class BadArgNumberParser
    {
        [Production("badargnumber : A B ")]
        public SubBadVisitor BadNonTermArg(Token<BadVisitorTokens> a, Token<BadVisitorTokens> b, object somethingThatCouldBeAContext, BadVisitor extraneousArg)
        {
            return null;
        }
        
        [Production("badargnumber2 : A B ")]
        public SubBadVisitor BadNonTermArg(Token<BadVisitorTokens> a)
        {
            return null;
        }

       
    }
    
    public class BadOptionArgParser
    {
        [Production("badoptionarg : a? B? (A B)? [A|B]?")]
        public SubBadVisitor BadOptionArg(BadVisitor a, ValueOption<Token<BadVisitorTokens>> b, Group<BadVisitor,BadVisitorTokens> c, string d)
        {
            return null;
        }
        
        [Production("a : A ")]
        public SubBadVisitor BadNonTermArg(Token<BadVisitorTokens> a)
        {
            return null;
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
            var instance = new BadVisitorReturnParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
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
        
        [Fact]
        public void TestBadVisitorTerminalArgument()
        {
            var instance = new BadTerminalArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badtermarg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.Contains("parameter aArg has incorrect type", result.Errors.First().Message);
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badtermarg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.Contains("parameter aArg has incorrect type", result.Errors.First().Message);
            
            
           
        }
        
        
        [Fact]
        public void TestBadVisitorNonTerminalArgument()
        {
            var instance = new BadNonTerminalArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badnontermarg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.Contains("parameter aArg has incorrect type", result.Errors.First().Message);
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badnontermarg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.Contains("parameter aArg has incorrect type", result.Errors.First().Message);
            
            
           
        }
        
        [Fact]
        public void TestBadManyArgument()
        {
            var instance = new BadManyArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badmanyarg");
            Assert.True(result.IsError);
            Assert.Equal(4,result.Errors.Count);
            
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter aArg has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter bArg has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter cArg has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter dArg has incorrect type")));
           
        }
        
        [Fact]
        public void TestBadGroupArgument()
        {
            var instance = new BadGroupArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badgrouparg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            Assert.Contains("parameter aArg has incorrect type",result.Errors.First().Message);
        }
        
        [Fact]
        public void TestBadArgumentNumber()
        {
            var instance = new BadArgNumberParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badargnumber");
            Assert.True(result.IsError);
            Assert.Equal(3, result.Errors.Count);
            Assert.True(result.Errors.Exists(x => x.Message.Contains("visitor BadNonTermArg for rule badargnumber : A B  has incorrect argument number : expected 2 or 3, found 4")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("visitor BadNonTermArg for rule badargnumber2 : A B  has incorrect argument number : expected 2 or 3, found 1")));
        }
        
        [Fact]
        public void TestBadOptionArgument()
        {
            var instance = new BadOptionArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>();
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badoptionarg");
            Assert.True(result.IsError);
            Assert.Equal(4,result.Errors.Count);
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter a has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter b has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter c has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter d has incorrect type")));
        }
    }
}