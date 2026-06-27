using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class SyntaxParseResult<IN, OUT> where IN : struct, Enum
    {
        private bool _isEnded;
        public ISyntaxNode<IN, OUT> Root { get; set; }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        
        
        private HashSet<UnexpectedTokenSyntaxError<IN>> Errors { get; set; } 

        public int EndingPosition { get; set; }

        public bool IsEnded
        {
            get => _isEnded;
            set => _isEnded = value;
        }

        private void InitErrors()
        {
            if (Errors == null)
            {
                Errors = new HashSet<UnexpectedTokenSyntaxError<IN>>();
            }
        }
        
        public void AddErrors(IList<UnexpectedTokenSyntaxError<IN>> errors)
        {
            if (errors != null)
            {
                InitErrors();
                foreach (var error in errors)
                {
                    AddError(error);
                }
            }
        }

        public void AddError(UnexpectedTokenSyntaxError<IN> error)
        {
            InitErrors();
            if (Errors.Any())
            {
                int compare = error.CompareTo(Errors.First());
                bool eq = error.Equals(Errors.First());
            }
            Errors.Add(error);
        }

        public void ClearErrors()
        {
            Errors.Clear();
        }

        public IList<UnexpectedTokenSyntaxError<IN>> GetErrors() =>
            Errors == null ? new List<UnexpectedTokenSyntaxError<IN>>() : Errors?.ToList();
        
        public List<LeadingToken<IN>> Expecting {get; set;}

        public bool HasByPassNodes { get; set; } = false;
        public bool UsesOperations { get; set; }

        public string Dump()
        {
            if (IsOk)
            {
                return "OK : \n" + Root.Dump("  ");
            }
            else
            {
                return "KO "+Errors.First().ErrorMessage;
            }
                
        }
    }
}