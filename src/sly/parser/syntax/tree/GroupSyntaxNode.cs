using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class GroupSyntaxNode<IN, OUT> : ManySyntaxNode<IN, OUT> where IN : struct
    {
        public GroupSyntaxNode(string name) : base(name)
        {
        }

        public GroupSyntaxNode(string name,  List<ISyntaxNode<IN, OUT>> children) : this(name)
        {
            Children.AddRange(children);
        }

    }
}