using expressionparser;
using simpleExpressionParser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.generator.visitor.dotgraph;
using Xunit;

namespace ParserTests
{
    public class GraphVizTests
    {

        [Fact]
        public void DiGraphTest()
        {
            
            DotGraph graph = new DotGraph("test",true);
            var leaf1 =  new DotNode("l1") {
                // Set all available properties
                Shape = "doublecircle",
                Label = "leaf1",
                FontColor = "black",
                Style = "",
                Height = 0.5f
            };
            var leaf2 =  new DotNode("l2") {
                // Set all available properties
                Shape = "doublecircle",
                Label = "leaf2",
                FontColor = "red",
                Style = "",
                Height = 0.5f
            };
            var root =  new DotNode("root") {
                // Set all available properties
                Shape = "ellipse",
                Label = "leaf1",
                FontColor = "blue",
                Style = "",
                Height = 0.5f
            };
            graph.Add(leaf1);
            graph.Add(leaf2);
            graph.Add(root);

            var edge1 = new DotArrow(root, leaf1)
            {
                ArrowHeadShape = "none"
            };
            
            var edge2 = new DotArrow(root, leaf2)
            {
                ArrowHeadShape = "normal"
            };
            
            graph.Add(edge1);
            graph.Add(edge2);

            string actual = graph.Compile().Replace("\r","").Replace("\n","");
            string expected =
                @"digraph test {l1 [  label=""leaf1"" shape=doublecircle fontcolor=black height=0.50]l2 [  label=""leaf2"" shape=doublecircle fontcolor=red height=0.50]root [  label=""leaf1"" shape=ellipse fontcolor=blue height=0.50]root->l1 [  arrowshape=none];root->l2 [  arrowshape=normal];}";
            Assert.Equal(expected,actual);

        }

        [Fact]
        public void SyntaxTreeGraphVizTest()
        {
            var StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var  Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            var result = Parser.Result.Parse("1+1");

            var tree = result.SyntaxTree;
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExpressionToken>();
            var root = graphviz.VisitTree(tree);
            string graph = graphviz.Graph.Compile();


        }
        
    }
}