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
            return Clauses.Select<IClause<T>, string>(c => c.Dump()).Aggregate<string>((d1, d2) => d1 + " " + d2);
        }
        
        public bool Equals(IClause<T> other)
        {
            if (other != null)
            {
                if (other is ClauseSequence<T> sequence)
                {
                    if (sequence.Clauses.Count == Clauses.Count)
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
                }
            }

            return false;
        }
    }
}