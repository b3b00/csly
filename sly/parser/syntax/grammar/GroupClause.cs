using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public class GroupClause<T,OUT> : IClause<T,OUT>
    {
        public GroupClause(IClause<T,OUT> clause)
        {
            Clauses = new List<IClause<T,OUT>> {clause};
        }
     
        public GroupClause(ChoiceClause<T,OUT> choices)
        {
            Clauses = new List<IClause<T,OUT>>() {choices};
        }

        public List<IClause<T,OUT>> Clauses { get; set; }

        [ExcludeFromCodeCoverage]
        public bool MayBeEmpty()
        {
            return true;
        }

        public void AddRange(GroupClause<T,OUT> clauses)
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