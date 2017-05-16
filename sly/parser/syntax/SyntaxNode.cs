using sly.parser.syntax;
using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class SyntaxNode<T> : ISyntaxNode<T>
    {

        public string Name {get; set;} 

        public List<ISyntaxNode<T>> Children {get; set;}

        public SyntaxNode(string name, List<ISyntaxNode<T>> children)
        {
            this.Name = name;
            this.Children = children;
        }

        public SyntaxNode(string name)
        {
            this.Name = name;
            this.Children = new List<ISyntaxNode<T>>();
        }

        public override string ToString()
        {
            string r = Name+"(\n";
            Children.ForEach(c => r += c.ToString() + ",\n");
            return r+"\n)";
        }

        public bool IsTerminal() {
            return false;
        }

    }
}