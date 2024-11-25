using System.Collections.Generic;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class SyntaxParseResult<IN> where IN : struct
    {
        public ISyntaxNode<IN> Root { get; set; }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        
        
        private List<UnexpectedTokenSyntaxError<IN>> Errors { get; set; } //= new List<UnexpectedTokenSyntaxError<IN>>();

        public int EndingPosition { get; set; }

        public bool IsEnded { get; set; }

        private void InitErrors()
        {
            if (Errors == null) { 
                Errors = new List<UnexpectedTokenSyntaxError<IN>>();
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
            if (!Errors.Contains(error))
            {
                // #493 : dedup errors 
                Errors.Add(error);
            }
        }

        public IList<UnexpectedTokenSyntaxError<IN>> GetErrors() => Errors;
        
        public List<LeadingToken<IN>> Expecting {get; set;}

        public void AddExpecting(LeadingToken<IN> expected)
        {
            // if (Expecting == null)
            // {
            //     Expecting = new List<LeadingToken<IN>>();
            // }
            // Expecting.Add(expected);
        }
        
        public void AddExpectings(IEnumerable<LeadingToken<IN>> expected)
        {
            // if (expected == null)
            // {
            //     return;
            // }
            // if (Expecting == null)
            // {
            //     Expecting = new List<LeadingToken<IN>>();
            // }
            // Expecting.AddRange(expected);
        }

        public bool HasByPassNodes { get; set; } = false;
        public bool UsesOperations { get; set; }
    }
}