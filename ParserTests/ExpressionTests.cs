using expressionparser;
using NUnit.Framework;
using sly.parser;
using sly.parser.generator;

namespace ParserTests
{
    [TestFixture]
    public class ExpressionTests
    {
        private readonly Parser<ExpressionToken> Parser;

        public ExpressionTests()
        {
            ExpressionParser parserInstance = new ExpressionParser();
            ParserBuilder builder = new ParserBuilder();
            Parser = builder.BuildParser<ExpressionToken>(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression");
        }

        [Test]
        public void TestSingleValue()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(1, (int)r.Result);
        }

        [Test]
        public void TestSingleNegativeValue()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("-1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(-1, (int)r.Result);
        }

        [Test]
        public void TestTermPlus()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1 + 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(2, (int)r.Result);
        }

        [Test]
        public void TestTermMinus()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1 - 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(0, (int)r.Result);
        }

        [Test]
        public void TestFactorTimes()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("2*2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(4, (int)r.Result);
        }

        [Test]
        public void TestFactorDivide()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("42/2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(21, (int)r.Result);
        }

        [Test]
        public void TestGroup()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("(2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(4, (int)r.Result);
        }

        [Test]
        public void TestGroup2()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("6 * (2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(24, (int)r.Result);
        }

        [Test]
        public void TestPrecedence()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("6 * 2 + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(14, (int)r.Result);
        }
    }
}
