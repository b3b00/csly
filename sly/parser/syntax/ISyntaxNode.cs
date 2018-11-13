namespace sly.parser.syntax
{
    public interface ISyntaxNode<IN> where IN : struct
    {
        string Name { get; }
        string Dump(string tab);
    }
}