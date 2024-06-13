using System.Text;
using sly.parser.generator.visitor.dotgraph;

namespace sly.parser.generator.visitor.mermaid
{
    public class MermaidArrow : IMermaid
    {
        public MermaidNode Source { get; private set; }
        public MermaidNode Destination { get; private set; }
        
        public MermaidArrow(MermaidNode src, MermaidNode dest)
        {
            Source = src;
            Destination = dest;
        }
        
      

        public string ArrowHeadShape { get; set; }
        public string ToGraph()
        {
            var builder = new StringBuilder();
            builder.Append($"{Source.Name}---{Destination?.Name}");
            return builder.ToString();
        }

    }
}