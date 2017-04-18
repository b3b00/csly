using System.Collections.Generic;
using parser.parsergenerator.syntax;
using cpg.parser.parsgenerator.syntax;

namespace parser.parsergenerator.parser
{
    public class SyntaxParseResult<T> {
        public IConcreteSyntaxNode<T> Root {get; set;}

        public bool IsError{get; set;}

        public List<string> Errors {get; set;}

        public int EndingPosition { get; set; }

    }
}