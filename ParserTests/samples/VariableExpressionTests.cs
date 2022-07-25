using System.Collections.Generic;
using expressionparser;
using expressionparser.model;
using NFluent;
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
            var build = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression"); 
            Parser = build.Result;
        }

        private readonly Parser<ExpressionToken, Expression> Parser;

        [Fact]
        public void TestFactorDivide()
        {
            var r = Parser.Parse("42/2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(21);
        }

        [Fact]
        public void TestFactorTimes()
        {
            var r = Parser.Parse("2*2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup()
        {
            var r = Parser.Parse("(2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup2()
        {
            var r = Parser.Parse("6 * (2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(24);
        }

        [Fact]
        public void TestPrecedence()
        {
            var r = Parser.Parse("6 * 2 + 2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(14);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            var r = Parser.Parse("-1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(-1);
        }


        [Fact]
        public void TestSingleValue()
        {
            var r = Parser.Parse("1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(1);
        }

        [Fact]
        public void TestTermMinus()
        {
            var r = Parser.Parse("1 - 1");
            Check.That(r.IsError).IsFalse();
                        Check.That(r.Result).IsNotNull();
                        Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(0);
        }

        [Fact]
        public void TestTermPlus()
        {
            var r = Parser.Parse("1 + 1");
           Check.That(r.IsError).IsFalse();
                       Check.That(r.Result).IsNotNull();
                       Check.That(r.Result.Evaluate(new ExpressionContext())).IsEqualTo(2);
        }


        [Fact]
        public void TestVariables()
        {
            var r = Parser.Parse("a * b + c");
            var context = new ExpressionContext(new Dictionary<string, int> {{"a", 6}, {"b", 2}, {"c", 2}});
            Check.That(r.IsError).IsFalse();
                        Check.That(r.Result).IsNotNull();
                        Check.That(r.Result.Evaluate(context)).IsEqualTo(14);
        }

        [Fact]
        public void TestVariablesAndNumbers()
        {
            var r = Parser.Parse("a * b + 2");
            var context = new ExpressionContext(new Dictionary<string, int> {{"a", 6}, {"b", 2}});            
            Check.That(r.IsError).IsFalse();
                                    Check.That(r.Result).IsNotNull();
                                    Check.That(r.Result.Evaluate(context)).IsEqualTo(14);
        }
    }
}
