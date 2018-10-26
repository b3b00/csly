using Xunit;
using sly.parser;
using sly.parser.generator;
using expressionparser;
using simpleExpressionParser;
using System.Collections.Generic;
using sly.buildresult;

namespace ParserTests
{
    
    public class ExpressionGeneratorTests
    {

        private BuildResult<Parser<ExpressionToken,int>> Parser;

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
            Assert.False(Parser.IsError);
            Assert.Equal(6, Parser.Result.Configuration.NonTerminals.Count);
            var nonterminals = new List<NonTerminal<ExpressionToken>>();
            foreach(var pair in Parser.Result.Configuration.NonTerminals)
            {
                nonterminals.Add(pair.Value);
            }
            NonTerminal<ExpressionToken> nt = nonterminals[0]; // operan
            Assert.Single(nt.Rules);
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
            Assert.Equal(3, nt.Rules.Count);
            Assert.Contains("100", nt.Name);
            Assert.Contains("MINUS", nt.Name);
            nt = nonterminals[5];
            Assert.Single(nt.Rules);
            Assert.Equal(StartingRule, nt.Name);
            Assert.Single(nt.Rules[0].Clauses);
        }


        [Fact]
        public void TestSingleValue()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1",StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1, r.Result);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("-1", StartingRule);
            Assert.False(r.IsError);            
            Assert.Equal(-1, r.Result);
        }

        [Fact]
        public void TestTermPlus()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1 + 1", StartingRule);
            Assert.False(r.IsError);            
            Assert.IsType < int>(r.Result);
            Assert.Equal(2, (int)r.Result);
        }

        [Fact]
        public void TestTermMinus()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1 - 1", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(0, r.Result);
        }

        [Fact]
        public void TestFactorTimes()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("2*2", StartingRule);
            Assert.False(r.IsError);            
            Assert.IsType < int>(r.Result);
            Assert.Equal(4, r.Result);
        }

        [Fact]
        public void TestFactorDivide()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("42/2", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(21, r.Result);
        }

        [Fact]
        public void TestUnaryPrecedence()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("-1 * 2", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(-2, r.Result);
        }
        
        [Fact]
        public void TestPostFix()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1++", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(2, r.Result);
        }
        


        [Fact]
        public void TestPrecedence()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("-1 + 2  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(5, r.Result);
        }

        [Fact]
        public void TestGroup()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("(-1 + 2)  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(3, r.Result);
        }

        [Fact]
        public void TestAssociativityTerm()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1 - 2 - 3", StartingRule); 
            Assert.False(r.IsError);
            Assert.Equal(1-2-3, r.Result);
            

            r = Parser.Result.Parse("1 - 2 - 3 - 4", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1 - 2 - 3 - 4, r.Result);


            r = Parser.Result.Parse("1 - 2 + 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1 - 2 + 3, r.Result);
        }

        [Fact]
        public void TestAssociativityFactor()
        {
            BuildParser();
            ParseResult<ExpressionToken, int> r = Parser.Result.Parse("1 / 2 / 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1 / 2 / 3, r.Result);


            r = Parser.Result.Parse("1 / 2 / 3 / 4", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1 / 2 / 3 / 4, r.Result);


            r = Parser.Result.Parse("1 / 2 * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1 / 2 * 3, r.Result);
        }


    }
}
