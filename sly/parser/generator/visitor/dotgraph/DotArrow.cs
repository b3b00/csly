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

        public string ArrowHeadShape { get; set; }
        public string ToGraph()
        {
            return $"{source.Name}->{destination.Name} [ arrowhead={ArrowHeadShape} ];";
        }
    }
}