using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class SyntaxParseResult<IN, OUT> where IN : struct, Enum
    {
        public ISyntaxNode<IN, OUT> Root { get; set; }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        
        
        public HashSet<UnexpectedTokenSyntaxError<IN>> Errors { get; set; } 

        public int EndingPosition { get; set; }

        public bool IsEnded { get; set; }

        private void InitErrors()
        {
            if (Errors == null)
            {
                Errors = new HashSet<UnexpectedTokenSyntaxError<IN>>();
            }
        }
        
        public void AddErrors(IList<UnexpectedTokenSyntaxError<IN>> errors)
        {
            InitErrors();
            foreach (var error in errors)
            {
                AddError(error);
            }
        }

        public void AddError(UnexpectedTokenSyntaxError<IN> error)
        {
            InitErrors();
            Errors.Add(error);
        }

        public IList<UnexpectedTokenSyntaxError<IN>> GetErrors() => Errors?.ToList();
        
        public List<LeadingToken<IN>> Expecting {get; set;}

        public bool HasByPassNodes { get; set; } = false;
        public bool UsesOperations { get; set; }
    }
}