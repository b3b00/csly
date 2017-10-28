using Xunit;
using sly.parser;
using sly.parser.generator;
using expressionparser;
using simpleExpressionParser;
using System.Collections.Generic;

namespace ParserTests
{
    
    public class ExpressionGeneratorTests
    {

        private Parser<ExpressionToken,int> Parser;

        private string StartingRule = "";
        public ExpressionGeneratorTests()
        {
            
        }


        private void BuildParser()
        {
            StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            SimpleExpressionParser parserInstance = new SimpleExpressionParser();
            ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);            
        }

        [Fact]
       public void TestBuild()
        {
            BuildParser();            
            Assert.Equal(6, Parser.Configuration.NonTerminals.Count);
            var nonterminals = new List<NonTerminal<ExpressionToken>>();
            foreach(var pair in Parser.Configuration.NonTerminals)
            {
                nonterminals.Add(pair.Value);
            }
            NonTerminal<ExpressionToken> nt = nonterminals[0]; // operan
            Assert.Equal(1, nt.Rules.Count);
            Assert.Equal("operand", nt.Name);
            nt = nonterminals[1];
            Assert.Equal(2, nt.Rules.Count);
            Assert.Contains("primary_value", nt.Name);            
            nt = nonterminals[2];
            Assert.Equal(3, nt.Rules.Count);
            Assert.Contains("10", nt.Name);
            Assert.Contains("PLUS", nt.Name);
            Assert.Contains("MINUS", nt.Name);
            nt = nonterminals[3];
            Assert.Equal(3, nt.Rules.Count);
            Assert.Contains("50", nt.Name);
            Assert.Contains("TIMES", nt.Name);
            Assert.Contains("DIVIDE", nt.Name);
            nt = nonterminals[4];
            Assert.Equal(2, nt.Rules.Count);
            Assert.Contains("100", nt.Name);
            Assert.Contains("MINUS", nt.Name);
            nt = nonterminals[5];
            Assert.Equal(1, nt.Rules.Count);
            Assert.Equal(StartingRule, nt.Name);
            Assert.Equal(1, nt.Rules[0].Clauses.Count);
        }


        [Fact]
        public void TestSingleValue()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("1",StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(1, r.Result);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("-1", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-1, r.Result);
        }

        [Fact]
        public void TestTermPlus()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("1 + 1", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(2, (int)r.Result);
        }

        [Fact]
        public void TestTermMinus()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("1 - 1", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(0, r.Result);
        }

        [Fact]
        public void TestFactorTimes()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("2*2", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(4, r.Result);
        }

        [Fact]
        public void TestFactorDivide()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("42/2", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(21, r.Result);
        }

        [Fact]
        public void TestUnaryPrecedence()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("-1 * 2", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-2, r.Result);
        }


        [Fact]
        public void TestPrecedence()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("-1 + 2  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(5, r.Result);
        }

        [Fact]
        public void TestGroup()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("(-1 + 2)  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(3, r.Result);
        }

        [Fact]
        public void TestAssociativity()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Parse("1 - 2 - 3", StartingRule); 
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.Equal(-4, r.Result);
            

            r = Parser.Parse("1 - 2 - 3 - 4", StartingRule);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            // should be ((1 - 2) - 3) - 4= (-1 -3) -4  = -4 -4 = -8  but is 1 - (2 - ( 3  - 4 ) = 1- (2 - (-1)) = 1 - 3 = -2
        }


    }
}
