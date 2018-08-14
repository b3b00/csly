using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace sly.parser.syntax
{

    public class ManySyntaxNode<IN> : SyntaxNode<IN> where IN : struct
    {

        public bool IsManyTokens { get; set; }
        
        public bool IsManyValues { get; set; }

        public bool IsManyGroups { get; set; }

        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode<IN>>())
        {

        }
        

        public void Add(ISyntaxNode<IN> child)
        {
            Children.Add(child);
        }

[ExcludeFromCodeCoverage]
        public override string Dump(string tab)
        {
            StringBuilder dump = new StringBuilder();

            dump.AppendLine($"{tab}MANY {Name} [");
            foreach (ISyntaxNode<IN> c in Children)
            {
                dump.AppendLine(c.Dump(tab + "\t"));
            }

            dump.AppendLine($"{tab}]");

            return dump.ToString();
        }


    }
}