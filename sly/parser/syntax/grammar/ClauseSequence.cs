using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public class ClauseSequence<T> : IClause<T>
    {
        public ClauseSequence(IClause<T> item)
        {
            Clauses = new List<IClause<T>>();
            Clauses.Add(item);
        }

        public List<IClause<T>> Clauses { get; set; }

        public bool MayBeEmpty()
        {
            return true;
        }


        public void AddRange(List<IClause<T>> clauses)
        {
            Clauses.AddRange(clauses);
        }

        public void AddRange(ClauseSequence<T> seq)
        {
            AddRange(seq.Clauses);
        }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return Clauses.Select(c => c.Dump()).Aggregate((d1, d2) => d1 + " " + d2);
        }
    }
}