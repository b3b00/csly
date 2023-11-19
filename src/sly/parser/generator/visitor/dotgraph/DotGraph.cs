using System.Collections.Generic;
using System.Linq;
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

        public List<DotNode> FindRoots()
        {
            var roots = edges.Where(x => !edges.Any(y => y.Destination?.Name == x.Source?.Name));
            return roots.Select(x => x.Source).ToList();
        }

        public IList<DotArrow> FindEgdes(DotNode node)
        {
            var nodeEdges = edges.Where(x => x.Source?.Name == node.Name);
            return nodeEdges.ToList();
        }

        public string Dump()
        {
            var roots = FindRoots();
            return string.Join("\n\n",roots.Select(x => Dump("",x)));
        }

        private string Dump(string tab, DotNode node)
        {
            if (node == null)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(tab)
                .AppendLine(node.Label);
            var edges = FindEgdes(node);
            if (edges != null && edges.Any())
            {
                foreach (var edge in edges)
                {
                    builder.AppendLine(Dump(tab + "\t", edge.Destination));
                }
            }
            return builder.ToString();
        }
        
      
        
        
    }
}