using System.Collections.Generic;
using parser.parsergenerator.syntax;
using cpg.parser.parsgenerator.syntax;
using cpg.parser.parsgenerator.parser;

namespace parser.parsergenerator.parser
{
    public class SyntaxParseResult<T> {
        public IConcreteSyntaxNode<T> Root {get; set;}

        public bool IsError{get; set;}

        public List<UnexpectedTokenSyntaxError<T>> Errors {get; set;}

        public int EndingPosition { get; set; }

        public bool IsEnded { get; set; }

    }
}