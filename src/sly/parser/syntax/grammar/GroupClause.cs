using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class GroupClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public GroupClause(IClause<IN,OUT> clause)
        {
            Clauses = new List<IClause<IN,OUT>> {clause};
        }
     
        public GroupClause(ChoiceClause<IN,OUT> choices)
        {
            Clauses = new List<IClause<IN,OUT>> {choices};
        }

        public List<IClause<IN,OUT>> Clauses { get; set; }

        [ExcludeFromCodeCoverage]
        public bool MayBeEmpty()
        {
            return true;
        }

        public void AddRange(GroupClause<IN,OUT> clauses)
        {
            Clauses.AddRange(clauses.Clauses);
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder dump = new StringBuilder();
            dump.Append("( ");
            dump.Append(Clauses.Select(c => c.Dump()).Aggregate((d1, d2) => d1 + " " + d2));
            dump.Append(" )");
            return dump.ToString();
        }


        public bool Equals(IClause<IN,OUT> other)
        {
            if (other is GroupClause<IN,OUT> group && group.Clauses.Count == Clauses.Count)
            {
                for (int i = 0; i < Clauses.Count; i++)
                {
                    if (!Clauses[i].Equals(group.Clauses[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Dump();
        }
    }
}