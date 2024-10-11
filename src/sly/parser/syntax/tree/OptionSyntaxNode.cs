using System.Collections.Generic;
using System.Reflection;

namespace sly.parser.syntax.tree
{
    public class OptionSyntaxNode<IN> : SyntaxNode<IN> where IN : struct
    {
        public bool IsGroupOption { get; set; } = false;

        public OptionSyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null) : base(
            name, children, visitor)
        { }
    }
}