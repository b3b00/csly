using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;
using sly.parser.llparser;
using sly.parser.syntax;


namespace ParserTests
{
    
    public class EBNFTests
    {

        public enum TokenType
        {
            a = 1,
            b = 2,
            c = 3,
            WS = 100,
            EOL = 101
        }


        [LexerConfigurationAttribute]
        public ILexer<TokenType> BuildJsonLexer(ILexer<TokenType> lexer)
        {
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.a, "a"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.b, "b"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.c, "c"));
           
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }


        [Reduction("R : A B c ")]
        public object R(string A, string B, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += B + ",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Reduction("A : a + ")]
        public object A(List<Token<TokenType>> astr)
        {
            string result = "A(";
            result +=(string) astr
                .Select(a => a.Value)
                .Aggregate<string>((a1, a2) => a1 + ", " + a2);
            result += ")";
            return result;
        }

        [Reduction("B : b * ")]
        public object B(List<Token<TokenType>> bstr)
        {
            if (bstr.Any())
            {
                string result = "B(";
                result += bstr
                    .Select(b => b.Value)
                    .Aggregate<string>((b1, b2) => b1 + ", " + b2);
                result += ")";
                return result;
            }
            return "B()";
        }

        private Parser<TokenType> Parser;
      
        public EBNFTests()
        {
            
        }

        private Parser<TokenType> BuildParser()
        {
            EBNFTests parserInstance = new EBNFTests();
            EBNFParserBuilder<TokenType> builder = new EBNFParserBuilder<TokenType>();

            EBNFTests parserTest = new EBNFTests();

            Parser = builder.BuildParser(parserTest, ParserType.EBNF_LL_RECURSIVE_DESCENT, "R");
            return Parser;
        }


        [Fact]
        public void TestParseBuild()
        {
            Parser = BuildParser();
            Assert.Equal(typeof(EBNFRecursiveDescentSyntaxParser<TokenType>),Parser.SyntaxParser.GetType());
            Assert.Equal(Parser.Configuration.NonTerminals.Count, 3);
            NonTerminal<TokenType> nt = Parser.Configuration.NonTerminals["R"];
            Assert.Equal(nt.Rules.Count, 1);
            nt = Parser.Configuration.NonTerminals["A"];
            Assert.Equal(nt.Rules.Count, 1);
            Rule<TokenType> rule = nt.Rules[0];
            Assert.Equal(rule.Clauses.Count,1);
            Assert.IsType(typeof(OneOrMoreClause<TokenType>),rule.Clauses[0]);
                nt = Parser.Configuration.NonTerminals["B"];
            Assert.Equal(nt.Rules.Count, 1);
            rule = nt.Rules[0];
            Assert.Equal(rule.Clauses.Count, 1);
            Assert.IsType(typeof(ZeroOrMoreClause<TokenType>), rule.Clauses[0]);
            ;
        }

        [Fact]
        public void TestOneOrMoreWithMany()
        {
            Parser = BuildParser();
            ParseResult<TokenType> result = Parser.Parse("aaa b c");
            Assert.False(result.IsError);
            Assert.IsType(typeof(string),result.Result);
            Assert.Equal("R(A(a,a,a),B(b),c)",result.Result.ToString().Replace(" ",""));
        }

        [Fact]
        public void TestOneOrMoreWithOne()
        {
            Parser = BuildParser();
            ParseResult<TokenType> result = Parser.Parse(" b c");
            Assert.True(result.IsError);
        }

        [Fact]
        public void TestZeroOrMoreWithOne()
        {
            Parser = BuildParser();
            ParseResult<TokenType> result = Parser.Parse("a b c");
            Assert.False(result.IsError);
            Assert.IsType(typeof(string), result.Result);
            Assert.Equal("R(A(a),B(b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithMany()
        {
            Parser = BuildParser();
            ParseResult<TokenType> result = Parser.Parse("a bb c");
            Assert.False(result.IsError);
            Assert.IsType(typeof(string), result.Result);
            Assert.Equal("R(A(a),B(b,b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithNone()
        {
            Parser = BuildParser();
            ParseResult<TokenType> result = Parser.Parse("a  c");
            Assert.False(result.IsError);
            Assert.IsType(typeof(string), result.Result);
            Assert.Equal("R(A(a),B(),c)", result.Result.ToString().Replace(" ", ""));
        }





    }
}
