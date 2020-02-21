using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class GroupSyntaxNode<IN> : ManySyntaxNode<IN> where IN : struct
    {
        public GroupSyntaxNode(string name) : base(name)
        {
        }

        public GroupSyntaxNode(string name,  List<ISyntaxNode<IN>> children) : this(name)
        {
            Children.AddRange(children);
        }

    }
}