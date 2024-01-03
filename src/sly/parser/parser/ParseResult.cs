using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class ParseResult<IN, OUT> where IN : struct
    {
        public OUT Result { get; set; }
        
        public ISyntaxNode<IN> SyntaxTree { get; set; }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        public List<ParseError> Errors { get; set; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsOk)
            {
                return "parse OK.";
            }
            else
            {
                return $"parse failed : {string.Join("\n", Errors.Select(x => x.ErrorMessage))}";
            }
        }
    }
}