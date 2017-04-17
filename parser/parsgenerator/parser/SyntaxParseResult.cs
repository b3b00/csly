using System.Collections.Generic;
using parser.parsergenerator.syntax;

namespace parser.parsergenerator.parser
{
    public class SyntaxParseResult<T> {
        public ConcreteSyntaxNode<T> Root {get; set;}

        public bool IsError{get; set;}

        public List<string> Errors {get; set;}

    }
}