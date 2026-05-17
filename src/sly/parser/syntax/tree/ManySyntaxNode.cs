using System;
using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class ManySyntaxNode<IN, OUT> : SyntaxNode<IN, OUT> where IN : struct, Enum
    {
        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode<IN, OUT>>())
        {
        }

        public ManySyntaxNode()
        {
            if (Children == null)
            {
                Children = new List<ISyntaxNode<IN, OUT>>();
            }
        }

        public void Initialize(string name)
        {
            Name = name;
            if (Children != null)
                Children.Clear();
            else
                Children = new List<ISyntaxNode<IN, OUT>>();
        }

        public bool IsManyTokens { get; set; }

        public bool IsManyValues { get; set; }

        public bool IsManyGroups { get; set; }

        public override bool IsEpsilon => Children.Count == 0;
        

        public void Add(ISyntaxNode<IN, OUT> child)
        {
            Children.Add(child);
        }
    }
}