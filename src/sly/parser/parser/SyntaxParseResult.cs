using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class SyntaxParseResult<IN, OUT> where IN : struct, Enum
    {
        /// <summary>
        /// List of alternative roots (for ambiguous grammars)
        /// If null or 1 element, no ambiguity
        /// </summary>
        public List<ISyntaxNode<IN, OUT>> AlternativeRoots { get; set; }

        /// <summary>
        /// used for ambiguity capture to return results with different ending positions
        /// </summary>
        public List<SyntaxParseResult<IN, OUT>> AllResults { get; set; }
        
        /// <summary>
        /// Indicates if this result contains ambiguous alternatives
        /// </summary>
        public bool HasAmbiguity => AlternativeRoots != null && AlternativeRoots.Count > 1;
        
        /// <summary>
        /// Information about detected ambiguities
        /// </summary>
        public List<AmbiguityInfo<IN, OUT>> Ambiguities { get; set; }
        
        /// <summary>
        /// Compatibility: Root points to the first alternative
        /// </summary>
        public ISyntaxNode<IN, OUT> Root 
        { 
            get => AlternativeRoots?.FirstOrDefault(); 
            set 
            {
                if (AlternativeRoots == null)
                    AlternativeRoots = new List<ISyntaxNode<IN, OUT>>();
                if (AlternativeRoots.Count == 0)
                    AlternativeRoots.Add(value);
                else
                    AlternativeRoots[0] = value;
            }
        }

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        
        
        private HashSet<UnexpectedTokenSyntaxError<IN>> Errors { get; set; } 

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

        public IList<UnexpectedTokenSyntaxError<IN>> GetErrors() => Errors?.ToList() ?? new List<UnexpectedTokenSyntaxError<IN>>();
        
        public List<LeadingToken<IN>> Expecting {get; set;}

        public bool HasByPassNodes { get; set; } = false;
        public bool UsesOperations { get; set; }
    }
}