namespace sly.parser.syntax.tree;

public class EmptyNode<IN> : ISyntaxNode<IN> where IN : struct
{
    public bool IsEpsilon => true;
    public bool Discarded => false;
    public string Name => "epsilon";


    public bool HasByPassNodes
    {
        get { return false; } 
        set { }
    }

    public string Dump(string tab)
    {
        return "epsilon";
    }

    public string ToJson(int index = 0)
    {
        return $@"""{index}.{Name} : {{}}";
    }
}