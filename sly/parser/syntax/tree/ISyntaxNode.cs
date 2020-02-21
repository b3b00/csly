namespace sly.parser.syntax.tree
{
    public interface ISyntaxNode<IN> where IN : struct
    {
        
        bool Discarded { get;  }
        string Name { get; }
    }
}