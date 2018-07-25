using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace sly.parser.syntax
{

    public class GroupSyntaxNode<IN> : ManySyntaxNode<IN> where IN : struct
    {

        public GroupSyntaxNode(string name) : base(name)
        {
        }
        
        public GroupSyntaxNode(string name, List<ISyntaxNode<IN>> children) : this(name)
        {
            this.Children.AddRange(children);
        }


    [ExcludeFromCodeCoverage]
        public override string Dump(string tab)
        {
            StringBuilder dump = new StringBuilder();

            dump.AppendLine($"{tab}GROUP {Name} [");
            foreach (ISyntaxNode<IN> c in Children)
            {
                dump.AppendLine(c.Dump(tab + "\t"));
            }

            dump.AppendLine($"{tab}]");

            return dump.ToString();
        }



    }
}