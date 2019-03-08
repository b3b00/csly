namespace sly.parser.syntax
{
    public interface ISyntaxNode<IN> where IN : struct
    {
        
        bool Discarded { get;  }
        string Name { get; }
        string Dump(string tab);
    }
}