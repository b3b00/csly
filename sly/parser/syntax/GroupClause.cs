using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax
{
    public class GroupClause<T> : IClause<T>
    {
        public GroupClause(IClause<T> clause)
        {
            Clauses = new List<IClause<T>> {clause};
        }

        public List<IClause<T>> Clauses { get; set; }

        [ExcludeFromCodeCoverage]
        public bool MayBeEmpty()
        {
            return true;
        }

        public void AddRange(GroupClause<T> clauses)
        {
            Clauses.AddRange(clauses.Clauses);
        }
    }
}