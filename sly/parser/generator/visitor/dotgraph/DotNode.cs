namespace sly.parser.generator.visitor.dotgraph
{
    public class DotNode : IDot
    {
        public DotNode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public object Shape { get; set; }
        public string Label { get; set; }
        public string FontColor { get; set; }
        public string Style { get; set; }
        public float Height { get; set; }


        public string ToGraph()
        {
            return $@"{Name} [ shape={Shape} label=""{Label}""];";
        }
    }
}