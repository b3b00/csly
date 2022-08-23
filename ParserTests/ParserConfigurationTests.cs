using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using expressionparser;
using NFluent;
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
        [Lexeme("c")] C = 3
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


    public enum RecursivityToken
    {
        [Lexeme(GenericToken.Identifier,IdentifierType.Alpha)]
        ID = 1,
        
        [Lexeme(GenericToken.KeyWord,"a")]
        A = 2,
        
        [Lexeme(GenericToken.KeyWord,"b")]
        B = 3
    }
    
    public class BnfRecursiveGrammar
    {
        [Production("first : second A")]
        public object FirstRecurse(object o, Token<RecursivityToken> a)
        {
            return o;
        } 
        
        [Production("second : third B")]
        public object SecondRecurse(object o, Token<RecursivityToken> b)
        {
            return o;
        }
        
        [Production("third : first A")]
        public object ThirdRecurse(object o, Token<RecursivityToken> a)
        {
            return o;
        }
    }
    
    public class EbnfRecursiveGrammar
    {
        [Production("first : second* third A[d]")]
        public object FirstRecurse(List<object> seconds, object third)
        {
            return third;
        } 
        
        
        
        [Production("second :  A[d]")]
        public object SecondRecurse()
        {
            return null;
        }
        
        [Production("third : first A[d]")]
        public object ThirdRecurse(object o)
        {
            return o;
        }
    }
    
    public class EbnfRecursiveOptionGrammar
    {
        
        [Production("first : second? third B[d]")]
        public object FirstRecurse2(ValueOption<object> optSecond, object third)
        {
            return null;
        }
        
        [Production("second :  A[d]")]
        public object SecondRecurse()
        {
            return null;
        }
        
        [Production("third : first A[d]")]
        public object ThirdRecurse(object o)
        {
            return o;
        }
    }
    
    public class EbnfRecursiveChoiceGrammar
    {
        
        [Production("first : second? [third|fourth] B[d]")]
        public object FirstRecurse2(ValueOption<object> optSecond, object third)
        {
            return null;
        }
        
        [Production("second :  A[d]")]
        public object SecondRecurse()
        {
            return null;
        }
        
        [Production("third : first A[d]")]
        public object ThirdRecurse(object o)
        {
            return o;
        }
        
        [Production("fourth :  A[d]")]
        public object fourthRecurse()
        {
            return null;
        }
    }
    
    public class EbnfRecursiveOptionalChoiceGrammar
    {
        
        [Production("first : second? [second|fourth]? third B[d]")]
        public object FirstRecurse2(ValueOption<object> optSecond, object third)
        {
            return null;
        }
        
        [Production("second :  A[d]")]
        public object SecondRecurse()
        {
            return null;
        }
        
        [Production("third : first A[d]")]
        public object ThirdRecurse(object o)
        {
            return o;
        }
        
        [Production("fourth :  A[d]")]
        public object fourthRecurse()
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
            var parserBuilder = new ParserBuilder<ExpressionToken, int>("en");
            var instance = new ParserConfigurationTests();
            var result = parserBuilder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "R");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).CountIs(2);
            
            var warnerrors = result.Errors.Where(e => e.Level == ErrorLevel.WARN).ToList();
            var errorerrors = result.Errors.Where(e => e.Level == ErrorLevel.ERROR).ToList();

            var expectations = new[]
            {
                (ErrorLevel.ERROR, ErrorCodes.PARSER_REFERENCE_NOT_FOUND, "R2"),
                (ErrorLevel.WARN, ErrorCodes.NOT_AN_ERROR, "R3")
            };
            
            
            Check.That(warnerrors).IsSingle();
            var warn = warnerrors[0];
            Check.That(warn.Code).IsEqualTo(ErrorCodes.NOT_AN_ERROR);
            Check.That(warn.Message).Contains("R3");
            Check.That(warn.Message).Contains("never used");
            
            Check.That(errorerrors).IsSingle();
            var error = errorerrors[0];
            Check.That(error.Code).IsEqualTo(ErrorCodes.PARSER_REFERENCE_NOT_FOUND);
            Check.That(error.Message).Contains("R2");
            Check.That(error.Message).Contains("not exist");
            
        }

        [Fact]
        public void TestLexerBuildErrors()
        {
            var result = new BuildResult<ILexer<BadTokens>>();
            result = LexerBuilder.BuildLexer(result);

            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).CountIs(2);
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
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badreturn");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_RETURN_TYPE);
            Check.That(result.Errors[0].Message).Contains("BadReturn");
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badreturn");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_RETURN_TYPE);
            Check.That(result.Errors[0].Message).Contains("BadReturn");
        }
        
        [Fact]
        public void TestBadVisitorTerminalArgument()
        {
            var instance = new BadTerminalArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badtermarg");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
            Check.That(result.Errors[0].Message).Contains("aArg");
            
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badtermarg");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
            Check.That(result.Errors[0].Message).Contains("aArg");
            
            
           
        }
        
        
        [Fact]
        public void TestBadVisitorNonTerminalArgument()
        {
            var instance = new BadNonTerminalArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "badnontermarg");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
            Check.That(result.Errors[0].Message).Contains("parameter aArg has incorrect type");
            
            result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badnontermarg");
            Check.That(result).Not.IsOkParser();
            Check.That(result.Errors).IsSingle();
            Check.That(result.Errors[0].Code).IsEqualTo(ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
            Check.That(result.Errors[0].Message).Contains("parameter aArg has incorrect type");
            
            
           
        }
        
        [Fact]
        public void TestBadManyArgument()
        {
            var instance = new BadManyArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badmanyarg");
            
            Assert.True(result.IsError);
            Assert.Equal(4,result.Errors.Count);
            
            //"visitor BadReturn for rule badtermarg :  A B ; parameter a has incorrect type : expected sly.lexer.Token`1[ParserTests.BadVisitorTokens], found SubBadVisitor"
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter aArg has incorrect type") && x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter bArg has incorrect type") && x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter cArg has incorrect type") && x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter dArg has incorrect type") && x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
           
        }
        
        [Fact]
        public void TestBadGroupArgument()
        {
            var instance = new BadGroupArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badgrouparg");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            Assert.Contains("parameter aArg has incorrect type",result.Errors.First().Message);
        }
        
        [Fact]
        public void TestBadArgumentNumber()
        {
            var instance = new BadArgNumberParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badargnumber");
            Assert.True(result.IsError);
            Assert.Equal(3, result.Errors.Count);
            Assert.Equal(2, result.Errors.Count(x => x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("visitor BadNonTermArg for rule badargnumber : A B  has incorrect argument number : expected 2 or 3, found 4")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("visitor BadNonTermArg for rule badargnumber2 : A B  has incorrect argument number : expected 2 or 3, found 1")));
        }
        
        [Fact]
        public void TestBadOptionArgument()
        {
            var instance = new BadOptionArgParser();
            ParserBuilder<BadVisitorTokens,BadVisitor> builder = new ParserBuilder<BadVisitorTokens, BadVisitor>("en");
            var result = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "badoptionarg");
            Assert.True(result.IsError);
            Assert.Equal(4,result.Errors.Count);
            Assert.Equal(4, result.Errors.Count(x => x.Code == ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter a has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter b has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter c has incorrect type")));
            Assert.True(result.Errors.Exists(x => x.Message.Contains("parameter d has incorrect type")));
        }

        [Fact]
        public void TestBasicRecursivity()
        {
            var builder = new ParserBuilder<RecursivityToken, object>();
            var  parserInstance = new BnfRecursiveGrammar();
            
            
            var Parser = builder.BuildParser(parserInstance,ParserType.LL_RECURSIVE_DESCENT,"clause");
            Assert.True(Parser.IsError);
            Assert.Single(Parser.Errors);
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE,Parser.Errors.First().Code);
        }
        
        [Fact]
        public void TestEbnfRecursivity()
        {
            var builder = new ParserBuilder<RecursivityToken, object>();
            var  parserInstance = new EbnfRecursiveGrammar();
            
            
            var Parser = builder.BuildParser(parserInstance,ParserType.EBNF_LL_RECURSIVE_DESCENT,"clause");
            Assert.True(Parser.IsError);
            Assert.Single(Parser.Errors);
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE,Parser.Errors.First().Code);
        }
        
        [Fact]
        public void TestEbnfOptionRecursivity()
        {
            var builder = new ParserBuilder<RecursivityToken, object>();
            var  parserInstance = new EbnfRecursiveOptionGrammar();
            
            
            var Parser = builder.BuildParser(parserInstance,ParserType.EBNF_LL_RECURSIVE_DESCENT,"clause");
            Assert.True(Parser.IsError);
            Assert.Single(Parser.Errors);
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE,Parser.Errors.First().Code);
        }
        
        [Fact]
        public void TestEbnfChoiceRecursivity()
        {
            var builder = new ParserBuilder<RecursivityToken, object>();
            var  parserInstance = new EbnfRecursiveChoiceGrammar();
            
            
            var Parser = builder.BuildParser(parserInstance,ParserType.EBNF_LL_RECURSIVE_DESCENT,"clause");
            Assert.True(Parser.IsError);
            Assert.Single(Parser.Errors);
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE,Parser.Errors.First().Code);
        }
        
        [Fact]
        public void TestEbnfOptionalChoiceRecursivity()
        {
            var builder = new ParserBuilder<RecursivityToken, object>();
            var  parserInstance = new EbnfRecursiveOptionalChoiceGrammar();
            
            
            var Parser = builder.BuildParser(parserInstance,ParserType.EBNF_LL_RECURSIVE_DESCENT,"clause");
            Assert.True(Parser.IsError);
            Assert.Single(Parser.Errors);
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE,Parser.Errors.First().Code);
        }
    }
}