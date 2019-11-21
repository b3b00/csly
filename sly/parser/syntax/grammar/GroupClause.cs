using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace sly.parser.syntax.grammar
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
        
        public string Dump()
        {
            StringBuilder dump = new StringBuilder();
            dump.Append("( ");
            dump.Append(Clauses.Select(c => c.Dump()).Aggregate((d1, d2) => d1 + " " + d2));
            dump.Append(" )");
            return dump.ToString();
        }
    }
}