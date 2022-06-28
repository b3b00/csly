using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    [DebuggerDisplay("{ToString()}")]
    public class GroupClause<T> : IClause<T> where T : struct
    {
        public GroupClause(IClause<T> clause)
        {
            Clauses = new List<IClause<T>> {clause};
        }
     
        public GroupClause(ChoiceClause<T> choices)
        {
            Clauses = new List<IClause<T>> {choices};
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
            dump.Append(Clauses.Select<IClause<T>, string>(c => c.Dump()).Aggregate<string>((d1, d2) => d1 + " " + d2));
            dump.Append(" )");
            return dump.ToString();
        }

        public override string ToString()
        {
            return Dump();
        }
    }
}