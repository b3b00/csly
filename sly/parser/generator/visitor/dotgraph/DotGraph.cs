using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator.visitor.dotgraph
{
    public class DotGraph
    {
        private readonly string GraphName;
        private readonly bool Directed;
        private List<DotNode> nodes;
        private List<DotArrow> edges;
        
        public DotGraph(string graphName, bool directed)
        {
            GraphName = graphName;
            Directed = directed;
            nodes = new List<DotNode>();
            edges = new List<DotArrow>();
        }

        public void Add(DotNode node)
        {
            nodes.Add(node);
        }

        public void Add(DotArrow edge)
        {
            edges.Add(edge);
        }

        public string Compile()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Directed ? "digraph" : "graph");
            builder.AppendLine($" {GraphName} {{");
            foreach (var node in nodes)
            {
                builder.AppendLine(node.ToGraph());
            }

            foreach (var edge in edges)
            {
                builder.AppendLine(edge.ToGraph());
            }
            builder.AppendLine("}");
            return builder.ToString();
        }
    }
}