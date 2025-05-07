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
            RecursiveParser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
            StackParser = builder.BuildParser(parserInstance, ParserType.LL_STACK, "expression").Result;
            
            
        }

        private readonly Parser<ExpressionToken, int> RecursiveParser;
        
        private readonly Parser<ExpressionToken, int> StackParser;

        [Fact]
        public void TestFactorDivide()
        {
            var r = RecursiveParser.Parse("42/2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(21);
        }
        
        [Fact]
        public void TestFactorDivideStack()
        {
            var r = StackParser.Parse("42/2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(21);
        }

        [Fact]
        public void TestFactorTimes()
        {
            var r = RecursiveParser.Parse("2*2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }
        
        [Fact]
        public void TestFactorTimesStack()
        {
            var r = StackParser.Parse("2*2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup()
        {
            var r = RecursiveParser.Parse("(2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }
        
        [Fact]
        public void TestGroupStack()
        {
            var r = StackParser.Parse("(2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestGroup2()
        {
            var r = RecursiveParser.Parse("6 * (2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(24);
        }
        
        [Fact]
        public void TestGroup2Stack()
        {
            var r = StackParser.Parse("6 * (2 + 2)");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(24);
        }

        [Fact]
        public void TestPrecedence()
        {
            var r = RecursiveParser.Parse("6 * 2 + 2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(14);
        }
        
        [Fact]
        public void TestPrecedenceStack()
        {
            var r = StackParser.Parse("6 * 2 + 2");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(14);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            var r = RecursiveParser.Parse("-1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(-1);
        }
        
        [Fact]
        public void TestSingleNegativeValueStack()
        {
            var r = StackParser.Parse("-1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(-1);
        }


        [Fact]
        public void TestSingleValue()
        {
            var r = RecursiveParser.Parse("1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(1);
        }
        
        [Fact]
        public void TestSingleValueStack()
        {
            var r = StackParser.Parse("1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(1);
        }

        [Fact]
        public void TestTermMinus()
        {
            var r = RecursiveParser.Parse("1 - 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(0);
        }
        
        [Fact]
        public void TestTermMinusStack()
        {
            var r = StackParser.Parse("1 - 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(0);
        }

        [Fact]
        public void TestTermPlus()
        {
            var r = RecursiveParser.Parse("1 + 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(2);
        }
        
        [Fact]
        public void TestTermPlusStack()
        {
            var r = StackParser.Parse("1 + 1");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo(2);
        }
        
        [Fact]
        public void TestIssue351NotReachingEOS()
        {
            var r = RecursiveParser.Parse("1 + 1 + 1");
            Check.That(r).IsOkParsing();
            
            r = RecursiveParser.Parse("1 + 1 + ");
            Check.That(r).Not.IsOkParsing();
            Check.That(r.Errors).CountIs(1);
            var error = r.Errors[0];
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }
        
        [Fact]
        public void TestIssue351NotReachingEOSStack()
        {
            var r = StackParser.Parse("1 + 1 + 1");
            Check.That(r).IsOkParsing();
            
            r = RecursiveParser.Parse("1 + 1 + ");
            Check.That(r).Not.IsOkParsing();
            Check.That(r.Errors).CountIs(1);
            var error = r.Errors[0];
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }
    }
}