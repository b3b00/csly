using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using System.Threading;
using expressionparser;
using expressionparser.model;

namespace ParserTests
{
    
    public class VariableExpressionTests
    {

        private Parser<ExpressionToken,Expression> Parser;
      
        public VariableExpressionTests()
        {
            VariableExpressionParser parserInstance = new VariableExpressionParser();
            ParserBuilder<ExpressionToken, Expression> builder = new ParserBuilder<ExpressionToken, Expression>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
        }


        [Fact]
        public void TestSingleValue()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(1, r.Result.Evaluate(new ExpressionContext())
            );
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            ParseResult <ExpressionToken,Expression> r = Parser.Parse("-1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-1, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestTermPlus()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("1 + 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(2, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestTermMinus()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("1 - 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(0, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestFactorTimes()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("2*2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(4, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestFactorDivide()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("42/2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(21, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestGroup()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("(2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(4, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestGroup2()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("6 * (2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(24, r.Result.Evaluate(new ExpressionContext()));
        }

        [Fact]
        public void TestPrecedence()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("6 * 2 + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(14, r.Result.Evaluate(new ExpressionContext()));
        }


        [Fact]
        public void TestVariables()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("a * b + c");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            ExpressionContext context = new ExpressionContext(new Dictionary<string, int>() { {"a",6}, {"b",2},{"c",2} });
            Assert.Equal(14, r.Result.Evaluate(context));
        }
        
        [Fact]
        public void TestVariablesAndNumbers()
        {
            ParseResult<ExpressionToken,Expression> r = Parser.Parse("a * b + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            ExpressionContext context = new ExpressionContext(new Dictionary<string, int>() { {"a",6}, {"b",2}});
            Assert.Equal(14, r.Result.Evaluate(context));
        }

      
    }
}
