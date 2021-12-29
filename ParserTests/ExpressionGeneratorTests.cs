using System.Collections.Generic;
using expressionparser;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{

    public class ExpressionGeneratorError
    {

        
        [Operation((int)ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation("MINUSCULE", Affix.InFix, Associativity.Left, 10)]
        public double BinaryTermExpression(double left, Token<ExpressionToken> operation, double right)
        {
            return 0;
        }

    }


    public class ShortOperationAttributesParser
    {
        [Infix((int) ExpressionToken.PLUS, Associativity.Right, 10)]
        [Infix("MINUS", Associativity.Left, 10)]
        public double BinaryTermExpression(double left, Token<ExpressionToken> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case ExpressionToken.PLUS:
                {
                    result = left + right;
                    break;
                }
                case ExpressionToken.MINUS:
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }


        [Infix((int) ExpressionToken.TIMES, Associativity.Right, 50)]
        [Infix("DIVIDE", Associativity.Left, 50)]
        public double BinaryFactorExpression(double left, Token<ExpressionToken> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case ExpressionToken.TIMES:
                {
                    result = left * right;
                    break;
                }
                case ExpressionToken.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }


        [Prefix((int) ExpressionToken.MINUS,  Associativity.Right, 100)]
        public double PreFixExpression(Token<ExpressionToken> operation, double value)
        {
            return -value;
        }

        [Postfix((int) ExpressionToken.FACTORIAL,  Associativity.Right, 100)]
        public double PostFixExpression(double value, Token<ExpressionToken> operation)
        {
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial = factorial * i;
            return factorial;
        }

        

        [Operand]
        [Production("double_value : DOUBLE")]
        public double OperandDouble(Token<ExpressionToken> value)
        {
            return value.DoubleValue;
        }
        
        [Operand]
        [Production("int_value : INT")]
        public double OperandInt(Token<ExpressionToken> value)
        {
            return value.DoubleValue;
        }

        [Operand]
        [Production("group : LPAREN ShortOperationAttributesParser_expressions RPAREN")]
        public double OperandGroup(Token<ExpressionToken> lparen, double value, Token<ExpressionToken> rparen)
        {
            return value;
        }
    }
    
    public class ExpressionGeneratorTests
    {
        private BuildResult<Parser<ExpressionToken, double>> Parser;

        private string StartingRule = "";


        private void BuildParser()
        {
            StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            Parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
        }

        [Fact]
        public void TestAssociativityFactor()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 / 2 / 3", StartingRule);
            Assert.False(r.IsError);
            Assert.Equal(1.0 / 2.0 / 3.0, r.Result);

            r = Parser.Result.Parse("(1 / 2 / 3)", StartingRule);
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
            var nonterminals = new List<NonTerminal<ExpressionToken,double>>();
            foreach (var pair in Parser.Result.Configuration.NonTerminals) nonterminals.Add(pair.Value);
            var nt = nonterminals[0]; // operan
            Assert.Single(nt.Rules);
            Assert.Equal("operand", nt.Name);
            nt = nonterminals[1];
            Assert.Equal(3, nt.Rules.Count);
            Assert.Contains("primary_value", nt.Name);
            nt = nonterminals[2];
            Assert.Single(nt.Rules);
            Assert.Contains("10", nt.Name);
            Assert.Contains("PLUS", nt.Name);
            Assert.Contains("MINUS", nt.Name);
            nt = nonterminals[3];
            Assert.Single(nt.Rules);
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

        [Fact]
        public void TestIssue184()
        {
            StartingRule = $"{typeof(Issue184ParserOne).Name}_expressions";
            var parserInstance = new Issue184ParserOne();
            var builder = new ParserBuilder<Issue184Token, double>();
            var issue184parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(issue184parser.IsOk);
            var c = issue184parser.Result.Parse(" 2 + 2");
            Assert.True(c.IsOk);
            Assert.Equal(4.0,c.Result);
            
            
            StartingRule = $"{typeof(Issue184Parser).Name}_expressions";
            var parserInstance2 = new Issue184Parser();
            var builder2 = new ParserBuilder<Issue184Token, double>();
            var issue184parser2 = builder.BuildParser(parserInstance2, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(issue184parser2.IsOk);
            var c2 = issue184parser2.Result.Parse(" 2 + 2");
            Assert.True(c2.IsOk);
            Assert.Equal(4.0,c2.Result);
            
            c2 = issue184parser2.Result.Parse(" 2 + 2 / 2");
            Assert.True(c2.IsOk);
            Assert.Equal(2 + 2 / 2 ,c2.Result);
            
            c2 = issue184parser2.Result.Parse(" 2 - 2 * 2");
            Assert.True(c2.IsOk);
            Assert.Equal(2 - 2 * 2 ,c2.Result);
        }

        [Fact]
        public void TestBadOperatorString()
        {
            var parserInstance = new ExpressionGeneratorError();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var exception = Assert.Throws<ParserConfigurationException>(() =>
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule));
            Assert.Contains("bad enum name MINUSCULE",exception.Message);
        }

        [Fact]
        public void TestShortOperationAttributes()
        {
            StartingRule = $"{typeof(ShortOperationAttributesParser).Name}_expressions";
            var parserInstance = new ShortOperationAttributesParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(buildResult.IsOk);
            var parser = buildResult.Result;
            Assert.NotNull(parser);
            var result = parser.Parse("-1 +2 * (5 + 6) - 4 ");
            Assert.True(result.IsOk);
            Assert.Equal(-1+2*(5+6)-4, result.Result);

        }
    }
}