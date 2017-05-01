using sly.parser.syntax;
using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class ConcreteSyntaxNode<T> : IConcreteSyntaxNode<T>
    {

        public string Name {get; set;} 

        public List<IConcreteSyntaxNode<T>> Children {get; set;}

        public ConcreteSyntaxNode(string name, List<IConcreteSyntaxNode<T>> children)
        {
            this.Name = name;
            this.Children = children;
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