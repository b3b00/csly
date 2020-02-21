using System.Text;

namespace sly.parser.generator.visitor.dotgraph
{
    public class DotArrow : IDot
    {
        private DotNode source;
        private DotNode destination;
        
        public DotArrow(DotNode src, DotNode dest)
        {
            source = src;
            destination = dest;
        }
        
        public string Attribute(string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return $" {name}={value}";
            }

            return "";
        }

        public string Attribute(string name, double value)
        {
            return Attribute(name, string.Format("{0:0.00}", value).Replace(",","."));
        }

        public string ArrowHeadShape { get; set; }
        public string ToGraph()
        {
            var builder = new StringBuilder();
            builder.Append($"{source.Name}->{destination?.Name} [ ");

            builder.Append(Attribute("arrowshape", ArrowHeadShape));
            
            builder.Append("];");
            return builder.ToString();
        }
    }
}