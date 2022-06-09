namespace sly.parser.syntax.tree
{
    public interface ISyntaxNode<IN> where IN : struct
    {
        
        bool Discarded { get;  }
        string Name { get; }
        
        bool HasByPassNodes { get; set; }
        
        string Dump(string tab);

        string ToJson(int index = 0);
        
    }
}