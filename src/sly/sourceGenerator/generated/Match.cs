using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.sourceGenerator.generated;

public class Match<I, O> where I : struct
{

    public int Position = 0;
    
    public bool Matched = false;

    public ISyntaxNode<I, O> Node;
    public Match(ISyntaxNode<I, O> node, int position)
    {
        Position = position;
        Node = node;
        Matched = true;
    }

    public Match()
    {
        Matched = false;
    }
}
