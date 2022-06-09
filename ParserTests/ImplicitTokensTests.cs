using System.IO;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests
{
    public class ImplicitTokensTests
    {
        private Parser<ImplicitTokensTokens, double> Parser;

        private BuildResult<Parser<ImplicitTokensTokens, double>> BuildParser()
        {
            var parserInstance = new ImplicitTokensParser();
            var builder = new ParserBuilder<ImplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");
            return result;
        }
        
        private BuildResult<Parser<ImplicitTokensTokens, double>> BuildExpressionParser()
        {
            var parserInstance = new ImplicitTokensExpressionParser();
            var builder = new ParserBuilder<ImplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(ImplicitTokensExpressionParser)+"_expressions");
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
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ImplicitTokensTokens>();
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
    }
    
}
