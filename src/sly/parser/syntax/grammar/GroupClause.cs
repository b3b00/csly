using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class GroupClause<T> : IClause<T> where T : struct
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
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder dump = new StringBuilder();
            dump.Append("( ");
            dump.Append(Clauses.Select(c => c.Dump()).Aggregate((d1, d2) => d1 + " " + d2));
            dump.Append(" )");
            return dump.ToString();
        }


        public bool Equals(IClause<T> other)
        {
            if (other is GroupClause<T> group && group.Clauses.Count == Clauses.Count)
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