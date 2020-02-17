using System.Collections.Generic;
using expressionparser;
using simpleExpressionParser;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public class ExpressionGeneratorTests
    {
        private BuildResult<Parser<ExpressionToken, double>> Parser;

        private string StartingRule = "";


        private void BuildParser()
        {
            StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
        }

        [Fact]
        public void TestAssociativityFactor()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 / 2 / 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 / 2.0 / 3.0, r.Result);


            r = Parser.Result.Parse("1 / 2 / 3 / 4", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 / 2.0 / 3.0 / 4.0, r.Result);


            r = Parser.Result.Parse("1 / 2 * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 / 2.0 * 3.0, r.Result);
        }

        [Fact]
        public void TestAssociativityTerm()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 - 2 - 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 - 2.0 - 3.0, r.Result);


            r = Parser.Result.Parse("1 - 2 - 3 - 4", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 - 2.0 - 3.0 - 4.0, r.Result);


            r = Parser.Result.Parse("1 - 2 + 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 - 2.0 + 3.0, r.Result);
        }

        [Fact]
        public void TestBuild()
        {
            BuildParser();
            Assert.False(Parser.IsError);
            Assert.Equal(6, Parser.Result.Configuration.NonTerminals.Count);
            var nonterminals = new List<NonTerminal<ExpressionToken>>();
            foreach (var pair in Parser.Result.Configuration.NonTerminals) nonterminals.Add(pair.Value);
            var nt = nonterminals[0]; // operan
            Assert.Single(nt.Rules);
            Assert.Equal("operand", nt.Name);
            nt = nonterminals[1];
            Assert.Equal(3, nt.Rules.Count);
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
        public void TestFactorDivide()
        {
            BuildParser();
            var r = Parser.Result.Parse("42/2", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(21, r.Result);
        }

        [Fact]
        public void TestFactorTimes()
        {
            BuildParser();
            var r = Parser.Result.Parse("2*2", StartingRule);
            Assert.False(r.IsError);
            Assert.IsType<double>(r.Result);
            Assert.Equal(4.0, r.Result);
        }

        [Fact]
        public void TestGroup()
        {
            BuildParser();
            var r = Parser.Result.Parse("(-1 + 2)  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(3.0, r.Result);
        }

        [Fact]
        public void TestPostFix()
        {
            BuildParser();
            var r = Parser.Result.Parse("10!", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(3628800.0, r.Result);
        }


        [Fact]
        public void TestPrecedence()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1 + 2  * 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(5, r.Result);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(-1, r.Result);
        }


        [Fact]
        public void TestSingleValue()
        {
            BuildParser();
            var r = Parser.Result.Parse("1", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1, r.Result);
        }

        [Fact]
        public void TestTermMinus()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 - 1", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(0, r.Result);
        }

        [Fact]
        public void TestTermPlus()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 + 1", StartingRule);
            Assert.False(r.IsError);
            Assert.IsType<double>(r.Result);
            Assert.Equal(2.0, r.Result);
        }

        [Fact]
        public void TestUnaryPrecedence()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1 * 2", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(-2, r.Result);
        }
    }
}