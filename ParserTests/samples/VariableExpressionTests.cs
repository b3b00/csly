using System.Collections.Generic;
using expressionparser;
using expressionparser.model;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class VariableExpressionTests
    {
        public VariableExpressionTests()
        {
            var parserInstance = new VariableExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, Expression>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
        }

        private readonly Parser<ExpressionToken, Expression> Parser;

        [Fact]
        public void TestFactorDivide()
        {
            var r = Parser.Parse("42/2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(21, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestFactorTimes()
        {
            var r = Parser.Parse("2*2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(4, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestGroup()
        {
            var r = Parser.Parse("(2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(4, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestGroup2()
        {
            var r = Parser.Parse("6 * (2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(24, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestPrecedence()
        {
            var r = Parser.Parse("6 * 2 + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(14, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            var r = Parser.Parse("-1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-1, r.Result.Evaluate(new ExpressionContext()));
        }


        [Fact]
        public void TestSingleValue()
        {
            var r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(1, r.Result.Evaluate(new ExpressionContext())
            );
        }

        [Fact]
        public void TestTermMinus()
        {
            var r = Parser.Parse("1 - 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(0, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestTermPlus()
        {
            var r = Parser.Parse("1 + 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(2, r.Result.Evaluate(new ExpressionContext()));
        }


        [Fact]
        public void TestVariables()
        {
            var r = Parser.Parse("a * b + c");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            var context = new ExpressionContext(new Dictionary<string, int> {{"a", 6}, {"b", 2}, {"c", 2}});
            Assert.Equal(14, r.Result.Evaluate(context));
        }

        [Fact]
        public void TestVariablesAndNumbers()
        {
            var r = Parser.Parse("a * b + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            var context = new ExpressionContext(new Dictionary<string, int> {{"a", 6}, {"b", 2}});
            Assert.Equal(14, r.Result.Evaluate(context));
        }
    }
}