using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
{

    public class ConcreteSyntaxNode<T> {

        public string Name {get; set;} 

        public List<ConcreteSyntaxNode<T>> Children {get; set;}

        public virtual bool IsTerminal() {
            return false;
        }

    }
}