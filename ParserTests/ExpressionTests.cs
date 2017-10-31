using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;

namespace ParserTests
{
    
    public class ExpressionTests
    {

        private Parser<ExpressionToken,int> Parser;
      
        public ExpressionTests()
        {
            ExpressionParser parserInstance = new ExpressionParser();
            ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
        }


        [Fact]
        public void TestSingleValue()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(1, r.Result);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            ParseResult <ExpressionToken,int> r = Parser.Parse("-1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-1, r.Result);
        }

        [Fact]
        public void TestTermPlus()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("1 + 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(2, (int)r.Result);
        }

        [Fact]
        public void TestTermMinus()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("1 - 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(0, r.Result);
        }

        [Fact]
        public void TestFactorTimes()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("2*2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(4, r.Result);
        }

        [Fact]
        public void TestFactorDivide()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("42/2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(21, r.Result);
        }

        [Fact]
        public void TestGroup()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("(2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(4, r.Result);
        }

        [Fact]
        public void TestGroup2()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("6 * (2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(24, r.Result);
        }

        [Fact]
        public void TestPrecedence()
        {
            ParseResult<ExpressionToken,int> r = Parser.Parse("6 * 2 + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(14, r.Result);
        }


        

      
    }
}
