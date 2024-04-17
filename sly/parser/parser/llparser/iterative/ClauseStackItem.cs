using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.iterative
{
    public class ClauseStackItem<IN> : IStackItem<IN> where IN : struct
    {
        public int Position { get; set; }
        
        public IList<Token<IN>> Tokens { get; set; }
        
        public IClause<IN> Clause { get; set; }
        
        public string NonTerminal { get; set; }
        
        public bool HasBackTracked { get; set; }

        public bool IsRuleTrial => false;

        public bool IsClause => true;

        
        public RuleStackItem<IN> Parent { get; set; }

        public bool HasParent => Parent != null;

        public bool IsRoot => false;
        
        private List<List<SyntaxParseResult<IN>>> Children { get; set; } 

        public ClauseStackItem(RuleStackItem<IN> parent, IClause<IN> clause, IList<Token<IN>> tokens, int position) 
        {
            Parent = parent;
            Clause = clause;
            Position = position;
            if (tokens == null)
            {
                Console.WriteLine($"ClauseStackItem : tokens are null");
            }
            
            Tokens = tokens;
        }
        
        public string Dump()
        {
            return Clause.Dump();
        }

    }
}