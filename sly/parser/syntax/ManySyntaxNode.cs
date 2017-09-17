using sly.parser.syntax;
using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class ManySyntaxNode<T> : SyntaxNode<T>
    {

        public bool IsManyTokens { get; set; }
        
        public bool IsManyValues { get { return !IsManyTokens; } set { IsManyTokens = !value; }}
        
        public ManySyntaxNode(string name) : base(name)
        {
        }

        public ManySyntaxNode(string name, List<ISyntaxNode<T>> children) : base(name,children)
        {
            this.Name = name;
            this.AddChildren(children);
        }
        
        

        public void Add(ISyntaxNode<T> child)
        {
            Children.Add(child);
        }

        public void AddRange(List<ISyntaxNode<T>> children)
        {
            Children.AddRange(children);
        }



        public override string ToString()
        {
            string r = Name+"(\n";
            Children.ForEach(c => r += c.ToString() + ",\n");
            return r+"\n)";
        }
        
    }
}