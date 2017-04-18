using cpg.parser.parsgenerator.syntax;
using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
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

        public bool IsTerminal() {
            return false;
        }

    }
}