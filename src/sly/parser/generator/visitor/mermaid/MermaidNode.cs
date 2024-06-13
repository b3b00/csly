using System.Text;

namespace sly.parser.generator.visitor.dotgraph {
    
    public enum MermaidNodeShape
    {
        Circle,
        DoubleCircle,
        Rhombus
    }

    public enum MermaidNodeStyle
    {
        Solid,
        Dotted
    }
    
    public class MermaidNode : IMermaid
    {
        public MermaidNode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public MermaidNodeShape Shape { get; set; }
        public string Label { get; set; }
        public string FontColor { get; set; }
        public MermaidNodeStyle Style { get; set; }
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
            builder.Append(Name);
            if (Shape == MermaidNodeShape.Circle)
            {
                builder.Append("(");
            }
            else if (Shape == MermaidNodeShape.Rhombus)
            {
                builder.Append("{");
            }
            else if (Shape == MermaidNodeShape.DoubleCircle)
            {
                builder.Append("((");
            }

            builder.Append(Label);    
            
            if (Shape == MermaidNodeShape.Circle)
            {
                builder.Append(")");
            }
            else if (Shape == MermaidNodeShape.Rhombus)
            {
                builder.Append("}");
            }
            else if (Shape == MermaidNodeShape.DoubleCircle)
            {
                builder.Append("))");
            }
            return builder.ToString();
        }
    }
}