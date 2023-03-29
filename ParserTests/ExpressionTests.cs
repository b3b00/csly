using expressionparser;
using NFluent;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public class ExpressionTests
    {
        public ExpressionTests()
        {
            var parserInstance = new ExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, int>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
        }

        private readonly Parser<ExpressionToken, int> Parser;

        [Fact]
        public void TestFactorDivide()
        {
            var r = Parser.Parse("42/2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(21);
        }

        [Fact]
        public void TestFactorTimes()
        {
            var r = Parser.Parse("2*2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup()
        {
            var r = Parser.Parse("(2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup2()
        {
            var r = Parser.Parse("6 * (2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(24);
        }

        [Fact]
        public void TestPrecedence()
        {
            var r = Parser.Parse("6 * 2 + 2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(14);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            var r = Parser.Parse("-1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(-1);
        }


        [Fact]
        public void TestSingleValue()
        {
            var r = Parser.Parse("1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(1);
        }

        [Fact]
        public void TestTermMinus()
        {
            var r = Parser.Parse("1 - 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(0);
        }

        [Fact]
        public void TestTermPlus()
        {
            var r = Parser.Parse("1 + 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(2);
        }
        
        [Fact]
        public void TestIssue351NotReachingEOS()
        {
            var r = Parser.Parse("1 + 1 + 1");
            Check.That(r.IsError).IsFalse();
            
            r = Parser.Parse("1 + 1 + ");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Errors).CountIs(1);
            
            
            
        }
    }
}