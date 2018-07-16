using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class GroupClause<T> : IClause<T>
    {
        
        public List<IClause<T>> Clauses { get; set; }

        
        public GroupClause(IClause<T> clause)
        {
            Clauses = new List<IClause<T>>() {clause};
        }

        public GroupClause(List<IClause<T>> clauses)
        {
            Clauses = clauses;
        }

        public void AddRange(GroupClause<T> clauses) {
            Clauses.AddRange(clauses.Clauses);
        }


        public bool MayBeEmpty()
        {
            return true;
        }

    
    }
}