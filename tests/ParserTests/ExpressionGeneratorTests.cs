using System;
using System.Collections.Generic;
using NFluent;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using ParserTests.Issue184;
using sly.parser.generator.visitor;
using sly.parser.syntax.tree;
using ExpressionToken = expressionparser.ExpressionToken;

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

    public class PrefixOperation
    {
        [Prefix("'##'",Associativity.Left,10)]
        [NodeName("hash-hash")]
        public string prefixHashHash(Token<SimpleExpressionToken> token, string operand)
        {
            return $"{token.Value}::{operand}";
        }
        
        [Prefix("'@@'",Associativity.Left,10)]
        [NodeName("at-at")]
        public string prefixAtAt(Token<SimpleExpressionToken> token, string operand)
        {
            return $"{token.Value}::{operand}";
        }
        
        

        [Operand]
        [Production("testOperand: INT")]
        public string Operand(Token<SimpleExpressionToken> token)
        {
            return token.Value;
        }
    }
    
    public class ExpressionGeneratorExplicitOperatorAsNames
    {

        
        [Operation((int)SimpleExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation("'MINUS'", Affix.InFix, Associativity.Left, 10)]
        [Operation("'TIMES'", Affix.InFix, Associativity.Left, 20)]
        public double BinaryTermExpression(double left, Token<SimpleExpressionToken> operation, double right)
        {
            if (operation.Value == "MINUS")
            {
                return left - right;
            }
            if (operation.Value == "TIMES")
            {
                return left * right;
            }
            return left + right;
        }
        
        [Prefix((int) SimpleExpressionToken.MINUS,  Associativity.Right, 100)]
        [NodeName("hash-hash")]
        public double PreFixExpression(Token<SimpleExpressionToken> operation, double value)
        {
            return -value;
        }

        [Operand]
        [Production("value : DOUBLE")]
        public double doubleValue(Token<SimpleExpressionToken> val)
        {
            return val.DoubleValue;
        }
        
        [Operand]
        [Production("value : INT")]
        public double intValue(Token<SimpleExpressionToken> val)
        {
            return val.DoubleValue;
        }
        
        [Operand]
        [Production("group : LPAREN ExpressionGeneratorExplicitOperatorAsNames_expressions RPAREN")]
        public double OperandGroup(Token<SimpleExpressionToken> lparen, double value, Token<SimpleExpressionToken> rparen)
        {
            return value;
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
        private BuildResult<Parser<simpleExpressionParser.ExpressionToken, double>> Parser;

        private string StartingRule = "";


        private void BuildParser()
        {
            StartingRule = $"{nameof(SimpleExpressionParser)}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<simpleExpressionParser.ExpressionToken, double>();
            Parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
        }

        [Fact]
        public void TestAssociativityFactor()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 / 2 / 3", StartingRule);
            Check.That(r).IsOkParsing();;
            var expected = 1.0 / 2.0 / 3.0;
            Check.That(r.Result).IsEqualTo(expected);
            

            r = Parser.Result.Parse("(1 / 2 / 3)", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(expected);
            

            r = Parser.Result.Parse("1 / 2 / 3 / 4", StartingRule);
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(1.0/2.0/3.0/4.0);


            r = Parser.Result.Parse("1 / 2 * 3", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(1.0 /2.0 * 3.0);
        }

        [Fact]
        public void TestAssociativityTerm()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 - 2 - 3", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(1.0-2.0-3.0);

            r = Parser.Result.Parse("1 - 2 - 3 - 4", StartingRule);
            Check.That(r).IsOkParsing();

            r = Parser.Result.Parse("1 - 2 + 3", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(1.0-2.0+3.0);
        }

        [Fact]
        public void TestBuild()
        {
            BuildParser();
            
            Check.That(Parser.Result.Configuration.NonTerminals).CountIs(7);
            var nonterminals = new List<NonTerminal<simpleExpressionParser.ExpressionToken, double>>();
            foreach (var pair in Parser.Result.Configuration.NonTerminals) nonterminals.Add(pair.Value);
            var nt = nonterminals[1]; // operand
            Check.That(nt.Rules).IsSingle();
            Check.That(nt.Name).IsEqualTo("operand");
            nt = nonterminals[2];
            Check.That(nt.Rules).CountIs(3);
            Check.That(nt.Name).Contains("primary_value");
            Check.That(nt.Rules[0].NodeName).IsEqualTo("prim_double");
            Check.That(nt.Rules[1].NodeName).IsEqualTo("prim_int");
            Check.That(nt.Rules[2].NodeName).IsEqualTo("prim_group");
            nt = nonterminals[3];
            Check.That(nt.Rules).CountIs(1);
            Check.That(nt.Name).Contains("PLUS");
            Check.That(nt.Name).Contains("MINUS");
            nt = nonterminals[4];
            Check.That(nt.Rules).IsSingle();
            Check.That(nt.Name).IsEqualTo("TIMES_DIVIDE");
            nt = nonterminals[5];
            Check.That(nt.Rules).CountIs(3);
            Check.That(nt.Name).Contains("MINUS");
            nt = nonterminals[6];
            Check.That(nt.Rules).IsSingle();
            Check.That(nt.Name).IsEqualTo(StartingRule);
            Check.That(nt.Rules[0].Clauses).IsSingle();
        }

        [Fact]
        public void TestFactorDivide()
        {
            BuildParser();
            var r = Parser.Result.Parse("42/2", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(21);
        }

        [Fact]
        public void TestFactorTimes()
        {
            BuildParser();
            var r = Parser.Result.Parse("2*2", StartingRule);
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(4.0);
        }

        [Fact]
        public void TestGroup()
        {
            BuildParser();
            var r = Parser.Result.Parse("(-1 + 2)  * 3!", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo((-1 + 2 ) * 6.0);
            
            var d = r.SyntaxTree.Dump("  ");
            Check.That(d).Contains("addition_or_substraction")
                .And.Contains("multiplication_or_division")
                .And.DoesNotContain("TIMES_DIVIDE");
            
            GraphVizEBNFSyntaxTreeVisitor<simpleExpressionParser.ExpressionToken, double> gvViz =
            new GraphVizEBNFSyntaxTreeVisitor<simpleExpressionParser.ExpressionToken, double>();
            gvViz.VisitTree(r.SyntaxTree);
            string graph = gvViz.Graph.Compile();
            
            Check.That(graph).Contains("addition_or_substraction")
                .And.Contains("multiplication_or_division")
                .And.DoesNotContain("TIMES_DIVIDE");
        }

        [Fact]
        public void TestPostFix()
        {
            BuildParser();
            var r = Parser.Result.Parse("10!", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(3628800.0);
        }


        [Fact]
        public void TestPrecedence()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1 + 2  * 3", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(5.0);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(-1.0);
        }


        [Fact]
        public void TestSingleValue()
        {
            BuildParser();
            var r = Parser.Result.Parse("1", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(1.0);
        }

        [Fact]
        public void TestTermMinus()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 - 1", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(0.0);
        }

        [Fact]
        public void TestTermPlus()
        {
            BuildParser();
            var r = Parser.Result.Parse("1 + 1", StartingRule);
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(2.0);
        }

        [Fact]
        public void TestUnaryPrecedence()
        {
            BuildParser();
            var r = Parser.Result.Parse("-1 * 2", StartingRule);
            Check.That(r).IsOkParsing();;
            Check.That(r.Result).IsEqualTo(-2.0);
        }

        [Fact]
        public void TestIssue184()
        {
            StartingRule = $"{nameof(Issue184ParserOne)}_expressions";
            var parserInstance = new Issue184ParserOne();
            var builder = new ParserBuilder<Issue184Token, double>();
            var issue184parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(issue184parser).IsOk();
            var c = issue184parser.Result.Parse(" 2 + 2");
            Check.That(c).IsOkParsing();
            Check.That(c.Result).IsEqualTo(4.0);
            
            
            StartingRule = $"{nameof(Issue184Parser)}_expressions";
            var parserInstance2 = new Issue184Parser();
            var builder2 = new ParserBuilder<Issue184Token, double>();
            var issue184parser2 = builder.BuildParser(parserInstance2, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(issue184parser2).IsOk();
            var c2 = issue184parser2.Result.Parse(" 2 + 2");
            Check.That(c).IsOkParsing();
            Check.That(c.Result).IsEqualTo(4.0);
            
            c2 = issue184parser2.Result.Parse(" 2 + 2 / 2");
            Check.That(c2).IsOkParsing();
            Check.That(c2.Result).IsEqualTo(2 + 2 / 2);
            
            c2 = issue184parser2.Result.Parse(" 2 - 2 * 2");
            Check.That(c2).IsOkParsing();
            Check.That(c2.Result).IsEqualTo(2 - 2 * 2);
        }

        [Fact]
        public void TestBadOperatorString()
        {
            var parserInstance = new ExpressionGeneratorError();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var exception = Check.ThatCode(() =>
                    builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule))
                .Throws<ParserConfigurationException>().Value;
            Check.That(exception.Message).Contains("bad enum name MINUSCULE");
        }

        [Fact]
        public void TestShortOperationAttributes()
        {
            StartingRule = $"{nameof(ShortOperationAttributesParser)}_expressions";
            var parserInstance = new ShortOperationAttributesParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            
            var result = parser.Parse("-1 +2 * (5 + 6) - 4 ");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(-1+2*(5+6)-4);

        }
        
        [Fact]
        public void TestExplicitOperatorAsNames()
        {
            StartingRule = $"{nameof(ExpressionGeneratorExplicitOperatorAsNames)}_expressions";
            var parserInstance = new ExpressionGeneratorExplicitOperatorAsNames();
            var builder = new ParserBuilder<SimpleExpressionToken, double>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            
            var result = parser.Parse("-1 +2 TIMES (5 + 6) MINUS 4 ");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(-1+2*(5+6)-4);
        }

        [Fact]
        public void TestPrefix()
        {
            StartingRule = $"{nameof(PrefixOperation)}_expressions";
            var parserInstance = new PrefixOperation();
            var builder = new ParserBuilder<SimpleExpressionToken, string>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            
            var result = parser.Parse("## 42");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("##::42");
            var tree = result.SyntaxTree;
            var syntaxTree = tree as SyntaxNode<SimpleExpressionToken, string>;
            Check.That(syntaxTree).IsNotNull();
            Check.That(syntaxTree.Children).CountIs(1);
            Check.That(syntaxTree.Name).IsEqualTo($"{nameof(PrefixOperation)}_expressions");
            var child = syntaxTree.Children[0] as SyntaxNode<SimpleExpressionToken, string>;
            Check.That(child).IsNotNull();
            Check.That(child.Name).IsEqualTo("hash-hash");
            Check.That(child.Children).CountIs(2);
            
            result = parser.Parse("@@ 42");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("@@::42");
            tree = result.SyntaxTree;
            syntaxTree = tree as SyntaxNode<SimpleExpressionToken, string>;
            Check.That(syntaxTree).IsNotNull();
            Check.That(syntaxTree.Children).CountIs(1);
            Check.That(syntaxTree.Name).IsEqualTo($"{nameof(PrefixOperation)}_expressions");
            child = syntaxTree.Children[0] as SyntaxNode<SimpleExpressionToken, string>;
            Check.That(child).IsNotNull();
            Check.That(child.Name).IsEqualTo("at-at");
            Check.That(child.Children).CountIs(2);
        }
    }
}
