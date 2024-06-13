using System.Collections.Generic;
using System.Linq;
using System.Text;
using sly.parser.generator.visitor.dotgraph;

namespace sly.parser.generator.visitor.mermaid
{
    public class MermaidGraph
    {
        private readonly string GraphName;
        private readonly bool Directed;
        private List<MermaidNode> nodes;
        private List<MermaidArrow> edges;
        
        public MermaidGraph(string graphName, bool directed)
        {
            GraphName = graphName;
            Directed = directed;
            nodes = new List<MermaidNode>();
            edges = new List<MermaidArrow>();
        }

        public void Add(MermaidNode node)
        {
            nodes.Add(node);
        }

        public void Add(MermaidArrow edge)
        {
            edges.Add(edge);
        }

        public string Compile()
        {
            StringBuilder builder = new StringBuilder();
            // builder.Append(Directed ? "digraph" : "graph");
            // builder.AppendLine($" {GraphName} {{");
            builder.AppendLine("flowchart TD");
            foreach (var node in nodes)
            {
                builder.AppendLine(node.ToGraph());
            }

            foreach (var edge in edges)
            {
                builder.AppendLine(edge.ToGraph());
            }
            
            return builder.ToString();
        }

        public List<MermaidNode> FindRoots()
        {
            var roots = edges.Where(x => !edges.Any(y => y.Destination?.Name == x.Source?.Name));
            return roots.Select(x => x.Source).ToList();
        }

        public IList<MermaidArrow> FindEgdes(MermaidNode node)
        {
            var nodeEdges = edges.Where(x => x.Source?.Name == node.Name);
            return nodeEdges.ToList();
        }

        public string Dump()
        {
            var roots = FindRoots();
            return string.Join("\n\n",roots.Select(x => Dump("",x)));
        }

        private string Dump(string tab, MermaidNode node)
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