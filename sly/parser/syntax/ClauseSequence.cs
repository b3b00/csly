using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class ClauseSequence<T> : IClause<T>
    {
        
        public List<IClause<T>> Clauses { get; set; }

        
        public ClauseSequence(IClause<T> item)
        {
            Clauses = new List<IClause<T>>();
            Clauses.Add(item);
        }

    
        public void AddRange(List<IClause<T>> clauses)
        {
            Clauses.AddRange(clauses);
        }

        public void AddRange(ClauseSequence<T> seq)
        {
            AddRange(seq.Clauses);
        }

        public bool MayBeEmpty()
        {
            return true;
        }

    
    }
}