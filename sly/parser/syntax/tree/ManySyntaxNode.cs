using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class ManySyntaxNode<IN> : SyntaxNode<IN> where IN : struct
    {
        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode<IN>>())
        {
        }

        public bool IsManyTokens { get; set; }

        public bool IsManyValues { get; set; }

        public bool IsManyGroups { get; set; }


        public void Add(ISyntaxNode<IN> child)
        {
            Children.Add(child);
        }
    }
}