using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace sly.parser.syntax.tree
{
    public class GroupSyntaxNode<IN> : ManySyntaxNode<IN> where IN : struct
    {
        public GroupSyntaxNode(string name) : base(name)
        {
        }

        public GroupSyntaxNode(string name, List<ISyntaxNode<IN>> children) : this(name)
        {
            Children.AddRange(children);
        }


        [ExcludeFromCodeCoverage]
        public override string Dump(string tab)
        {
            var dump = new StringBuilder();

            dump.AppendLine($"{tab}GROUP {Name} [");
            foreach (var c in Children) dump.AppendLine(c.Dump(tab + "\t"));

            dump.AppendLine($"{tab}]");

            return dump.ToString();
        }
    }
}