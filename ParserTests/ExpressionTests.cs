using System.Linq;
using Xunit;
using cpg.parser.parsgenerator.parser;
using lexer;
using parser.parsergenerator.generator;
using System.Collections.Generic;
using expressionparser;

namespace ParserTests
{
    
    public class ExpressionTests
    {

        private static Parser<ExpressionToken> Parser;

        private static Lexer<ExpressionToken> Lexer;

      
        public ExpressionTests()
        {
            Lexer = ExpressionParser.BuildExpressionLexer(new Lexer<ExpressionToken>());
            Parser = ParserGenerator.BuildParser<ExpressionToken>(typeof(ExpressionParser), ParserType.RECURSIVE_DESCENT, "expression");
        }

        private object Parse(string expression )
        {
            List<Token<ExpressionToken>> tokens = Lexer.Tokenize(expression).ToList<Token<ExpressionToken>>();
            object r = Parser.Parse(tokens);
            return r;
        }

        [Fact]
        public void TestSingleValue()
        {
            object r = Parse("1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(1, (int)r);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            object r = Parse("-1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(-1, (int)r);
        }

        [Fact]
        public void TestTermPlus()
        {
            object r = Parse("1 + 1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(2, (int)r);
        }

        [Fact]
        public void TestTermMinus()
        {
            object r = Parse("1 - 1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(0, (int)r);
        }

        [Fact]
        public void TestFactorTimes()
        {
            object r = Parse("2*2");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(4, (int)r);
        }

        [Fact]
        public void TestFactorDivide()
        {
            object r = Parse("42/2");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(21, (int)r);
        }

        [Fact]
        public void TestGroup()
        {
            object r = Parse("(2 + 2)");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(4, (int)r);
        }

        [Fact]
        public void TestGroup2()
        {
            object r = Parse("6 * (2 + 2)");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(24, (int)r);
        }

        [Fact]
        public void TestPrecedence()
        {
            object r = Parse("6 * 2 + 2");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);
            Assert.Equal(14, (int)r);
        }

    }
}
