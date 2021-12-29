using System.Collections.Generic;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class ParseResult<IN, OUT> where IN : struct
    {
        public OUT Result { get; set; }
        
        public ISyntaxNode<IN, OUT> SyntaxTree { get; set; }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        public List<ParseError> Errors { get; set; }
    }
}