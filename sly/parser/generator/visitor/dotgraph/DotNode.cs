using System.Text;

namespace sly.parser.generator.visitor.dotgraph
{
    public class DotNode : IDot
    {
        public DotNode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string Shape { get; set; }
        public string Label { get; set; }
        public string FontColor { get; set; }
        public string Style { get; set; }
        public double Height { get; set; }


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

        public string ToGraph()
        {
            var builder = new StringBuilder();
            builder.Append(Name).Append(" [ ");
            builder.Append(Attribute("label", $@"""{Label}"""));
            builder.Append(Attribute("shape", Shape));
            builder.Append(Attribute("fontcolor", FontColor));
            builder.Append(Attribute("height", Height));
            builder.Append(Attribute("style", Style));
            builder.Append("]");
            return builder.ToString();
        }
    }
}