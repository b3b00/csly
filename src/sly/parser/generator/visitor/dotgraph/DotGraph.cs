using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.parser.generator.visitor.dotgraph
{
    public class DotGraph
    {
        private readonly string _graphName;
        private readonly bool _directed;
        private readonly List<DotNode> _nodes;
        private readonly List<DotArrow> _edges;
        
        public DotGraph(string graphName, bool directed)
        {
            _graphName = graphName;
            _directed = directed;
            _nodes = new List<DotNode>();
            _edges = new List<DotArrow>();
        }

        public void Add(DotNode node)
        {
            _nodes.Add(node);
        }

        public void Add(DotArrow edge)
        {
            _edges.Add(edge);
        }

        public string Compile()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(_directed ? "digraph" : "graph");
            builder.AppendLine($" {_graphName} {{");
            foreach (var node in _nodes)
            {
                builder.AppendLine(node.ToGraph());
            }

            foreach (var edge in _edges)
            {
                builder.AppendLine(edge.ToGraph());
            }
            builder.AppendLine("}");
            return builder.ToString();
        }

        public List<DotNode> FindRoots()
        {
            var roots = _edges.Where(x => !_edges.Any(y => y.Destination?.Name == x.Source?.Name));
            return roots.Select(x => x.Source).ToList();
        }

        public IList<DotArrow> FindEgdes(DotNode node)
        {
            var nodeEdges = _edges.Where(x => x.Source?.Name == node.Name);
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