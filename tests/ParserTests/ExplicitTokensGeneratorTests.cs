using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using csly.whileLang.model;
using ExplicitTokens;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests
{

  
    
    public class ExplicitTokensGeneratorTests
    {
       private BuildResult<Parser<ExplicitTokensTokens, double>> BuildParser()
        {
            var generator = new ExplicitTokensParserGenerator();
            var result = generator.GetParser();
            return result;
        }
        
        private BuildResult<Parser<ExplicitTokensTokens, double>> BuildExpressionParser()
        {
            var generator = new ExplicitTokensExpressionParserGenerator();
            var result = generator.GetParser(); 
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
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExplicitTokensTokens,double>();
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
