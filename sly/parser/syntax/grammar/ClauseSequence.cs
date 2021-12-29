using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public class ClauseSequence<T,OUT> : IClause<T,OUT>
    {
        public ClauseSequence(IClause<T,OUT> item)
        {
            Clauses = new List<IClause<T,OUT>>();
            Clauses.Add(item);
        }

        public List<IClause<T,OUT>> Clauses { get; set; }

        public bool MayBeEmpty()
        {
            return true;
        }


        public void AddRange(List<IClause<T,OUT>> clauses)
        {
            Clauses.AddRange(clauses);
        }

        public void AddRange(ClauseSequence<T,OUT> seq)
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