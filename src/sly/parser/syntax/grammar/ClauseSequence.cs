using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public sealed class ClauseSequence<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public ClauseSequence(IClause<IN,OUT> item)
        {
            Clauses = new List<IClause<IN,OUT>>();
            Clauses.Add(item);
        }

        public List<IClause<IN,OUT>> Clauses { get; set; }

        public bool MayBeEmpty()
        {
            return true;
        }


        public void AddRange(List<IClause<IN,OUT>> clauses)
        {
            Clauses.AddRange(clauses);
        }

        public void AddRange(ClauseSequence<IN,OUT> seq)
        {
            AddRange(seq.Clauses);
        }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return Clauses.Select(c => c.Dump()).Aggregate((d1, d2) => d1 + " " + d2);
        }
        
        public bool Equals(IClause<IN,OUT> other)
        {
            if (other is ClauseSequence<IN,OUT> sequence && sequence.Clauses.Count == Clauses.Count)
            {
                for (int i = 0; i < Clauses.Count; i++)
                {
                    if (!Clauses[i].Equals(sequence.Clauses[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}