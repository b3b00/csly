using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using csly.whileLang.model;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests
{

    public enum Lex
    {
        Nop = 0,
        [AlphaId]
        Id = 1,
        
        [Double]
        Dbl = 2
    }

    public class Parse
    {
        [Production("program : statement*")]
        public string Program(List<string> statements)
        {
            StringBuilder builder = new StringBuilder();
            bool first = true;
            builder.Append("(");
            foreach (var statement in statements)
            {
                builder.Append($"{(first?"":",")}({statement})");
                first = false;
            }
            builder.Append(")");
            return builder.ToString();
        }

        [Production("statement : Id '='[d] Parse_expressions ")]
        public string Assignment(Token<Lex> id, string expression)
        {
            return $"{id.Value} = {expression}";
        }
        
        [Production("condition : Id '=='[d] Parse_expressions ")]
        public string Condition(Token<Lex> id, string expression)
        {
            return $"{id.Value} == {expression}";
        }

        [Production("statement : 'if'[d] condition 'then'[d] statement 'else'[d] statement")]
        public string IfThenElse(string condition, string thenStatement, string elseStatement)
        {
            return $"condition:({condition},({thenStatement}),({elseStatement}))";
        }

        [Operand]
        [Production("operand : Id")]
        [Production("operand : Dbl")]
        public string Operand(Token<Lex> oper)
        {
            return oper.Value;
        }

        [Infix("'+'", Associativity.Left, 10)]
        public string Plus(string left, Token<Lex> oper, string right)
        {
            return $"( {left} + {right} )";
        }
        
        [Infix("'*'", Associativity.Left, 20)]
        public string Times(string left, Token<Lex> oper, string right)
        {
            return $"( {left} * {right} )";
        }
        
        
        
    }
    
    public class ExplicitTokensTests
    {
        private BuildResult<Parser<ExplicitTokensTokens, double>> BuildParser()
        {
            var parserInstance = new ExplicitTokensParser();
            var builder = new ParserBuilder<ExplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");

            
            return result;
        }
        
        private BuildResult<Parser<ExplicitTokensTokens, double>> BuildExpressionParser()
        {
            var parserInstance = new ExplicitTokensExpressionParser();
            var builder = new ParserBuilder<ExplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(ExplicitTokensExpressionParser)+"_expressions");
            var dump = result.Result.Configuration.Dump();
            return result;
        }

        [Fact]
        public void BuildParserTest()
        {
            var parser = BuildParser();
            Check.That(parser.IsOk).IsTrue();
            Check.That(parser.Result).IsNotNull();
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            
            
            
            Check.That(r).IsOkParsing();
            // grammar is left associative so expression really is 
            // (2.0 - (2.0 + (bozzo  + Test))) = 2 - ( 2 + (42 + 0)) = 2 - (2 + 42) = 2 - 44 = -42
            Check.That(r.Result).IsEqualTo(-42.0d);
        }
        
        [Fact]
        public void BuildExpressionParserTest()
        {
            var parser = BuildExpressionParser();
            Check.That(parser.IsOk).IsTrue();
            Check.That(parser.Result).IsNotNull();
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            Check.That(r).IsOkParsing();
            var tree = r.SyntaxTree;
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExplicitTokensTokens>();
            var dump = tree.Dump("\t");
            var json = $@"{{
{tree.ToJson()}
}}";
            
            var root = graphviz.VisitTree(tree);
            string graph = graphviz.Graph.Compile();
            Check.That(graph).Contains(@"label=""\""bozzo\""""")
                .And.Contains(@"label=""\""+\""""");
            
            Check.That(r.Result).IsEqualTo(2 - 2 + 42 + 0);
        }

        [Fact]
        public void TestErrorWhenUsingImplicitTokensAndRegexLexer()
        {
            var parserInstance = new RegexLexAndExplicitTokensParser();
            var builder = new ParserBuilder<RegexLexAndExplicitTokensLexer, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(RegexLexAndExplicitTokensParser)+"_expressions");
            Check.That(result.IsError).IsTrue();
            Check.That(result.Errors).CountIs(1);
            Check.That(result.Errors.First().Code).IsEqualTo(ErrorCodes.LEXER_CANNOT_USE_IMPLICIT_TOKENS_WITH_REGEX_LEXER);
        }
        
        [Fact]
        public void TestNoIdentifierPatternSuppliedWithImplicitTokens()
        {
            var parserInstance = new NoIdentifierParser();
            var builder = new ParserBuilder<NoIdentifierLexer, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsNotNull();
            var r = result.Result.Parse("test 1 test 2 test 3");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsEqualTo("test:1,test:2,test:3");
        }
        
        [Fact]
        public void Test()
        {
            var parserInstance = new Parse();
            var builder = new ParserBuilder<Lex, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            Check.That(result).IsOk();
            var r = result.Result.Parse(@"
if a == 1.0 then
    b = 1.0 + 2.0 * 3.0
else 
    b = 2.0 + a
c = 3.0
");
            Check.That(r).IsOkParsing();
            //"(condition:(a == 1.0,(b = ( 1.0 + ( 2.0 * 3.0 ) )),(b = ( 2.0 + a ))))(c = 3.0)"
            Check.That(r.Result).IsEqualTo("((condition:(a == 1.0,(b = ( 1.0 + ( 2.0 * 3.0 ) )),(b = ( 2.0 + a )))),(c = 3.0))");
        }
        
    }
    
}
