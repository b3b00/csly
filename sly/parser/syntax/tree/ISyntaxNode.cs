namespace sly.parser.syntax.tree
{
    public interface ISyntaxNode<IN, OUT> where IN : struct
    {
        
        bool Discarded { get;  }
        string Name { get; }
        
        bool HasByPassNodes { get; set; }
        
        string Dump(string tab);
    }
}