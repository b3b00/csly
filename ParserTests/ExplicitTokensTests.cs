using System.Collections.Generic;
using System.IO;
using System.Linq;
using csly.whileLang.model;
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
            return string.Join("\n", statements);
        }

        [Production("statement : Id '='[d] Dbl ")]
        public string Assignment(Token<Lex> id, Token<Lex> dbl)
        {
            return $"{id.Value} = {dbl.DoubleValue}";
        }
        
        [Production("condition : Id '=='[d] Dbl ")]
        public string Condition(Token<Lex> id, Token<Lex> dbl)
        {
            return $"{id.Value} == {dbl.DoubleValue}";
        }

        [Production("statement : 'if'[d] condition 'then'[d] statement 'else'[d] statement")]
        public string IfThenElse(string condition, string thenStatement, string elseStatement)
        {
            return $@"{condition} :
    - {thenStatement}
    - {elseStatement}";
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
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            Assert.True(r.IsOk);
            // grammar is left associative so expression really is 
            // (2.0 - (2.0 + (bozzo  + Test))) = 2 - ( 2 + (42 + 0)) = 2 - (2 + 42) = 2 - 44 = -42
            Assert.Equal(-42.0,r.Result);
        }
        
        [Fact]
        public void BuildExpressionParserTest()
        {
            var parser = BuildExpressionParser();
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            var tree = r.SyntaxTree;
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExplicitTokensTokens>();
            var dump = tree.Dump("\t");
            // File.Delete(@"c:\temp\tree.txt");
            // File.WriteAllText(@"c:\temp\tree.txt",dump);
            //
            var json = $@"{{
{tree.ToJson()}
}}";
            // File.Delete(@"c:\temp\tree.json");
            // File.WriteAllText(@"c:\temp\tree.json",json);
            //
            var root = graphviz.VisitTree(tree);
            string graph = graphviz.Graph.Compile();
            // File.Delete("c:\\temp\\tree.dot");
            // File.AppendAllText("c:\\temp\\tree.dot", graph);
            Assert.True(r.IsOk);
             
            
            Assert.Equal(2 - 2 + 42 + 0,r.Result);
        }

        [Fact]
        public void TestErrorWhenUsingImplicitTokensAndRegexLexer()
        {
            var parserInstance = new RegexLexAndExplicitTokensParser();
            var builder = new ParserBuilder<RegexLexAndExplicitTokensLexer, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(RegexLexAndExplicitTokensParser)+"_expressions");
            Assert.True(result.IsError);
            Assert.Single(result.Errors);
            Assert.Equal(ErrorCodes.LEXER_CANNOT_USE_IMPLICIT_TOKENS_WITH_REGEX_LEXER,result.Errors.First().Code);
        }
        
        [Fact]
        public void TestNoIdentifierPatternSuppliedWithImplicitTokens()
        {
            var parserInstance = new NoIdentifierParser();
            var builder = new ParserBuilder<NoIdentifierLexer, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            var r = result.Result.Parse("test 1 test 2 test 3");
            Assert.False(r.IsError);
            Assert.Equal("test:1,test:2,test:3",r.Result);
        }
        
        [Fact]
        public void Test()
        {
            var parserInstance = new Parse();
            var builder = new ParserBuilder<Lex, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            var r = result.Result.Parse(@"
if a == 1.0 then
    b = 1.0
else 
    b = 2.0
c = 3.0
");
            Assert.False(r.IsError);
        }
        
    }
    
}
