using System;
using System.Collections.Generic;
using lexer;

namespace parser.parsergenerator.syntax
{

    public class ConcreteSyntaxLeaf<T>: ConcreteSyntaxNode<T> {

        public Token<T> Token {get; set;}

        public bool IsTerminal() {
            return true;
        }

    }
}