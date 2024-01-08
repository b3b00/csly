using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public sealed class ChoiceClause<T> : IClause<T> where T : struct
    {

        public bool IsDiscarded { get; set; } = false;
        public bool IsTerminalChoice => Choices.Select(c => c is TerminalClause<T>).Aggregate((x, y) => x && y);
        public bool IsNonTerminalChoice => Choices.Select(c => c is NonTerminalClause<T>).Aggregate((x, y) => x && y);
            
        public  List<IClause<T>> Choices { get; }
        public ChoiceClause(IClause<T> clause)
        {
            Choices = new List<IClause<T>> {clause};
        }
        
        public ChoiceClause(List<IClause<T>> choices)
        {
            Choices = choices;
        }
        
        public ChoiceClause(IClause<T> choice, List<IClause<T>> choices) : this(choice)
        {
            Choices.AddRange(choices);
        }


        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var choices = string.Join(" | ", Choices.Select(c => c.Dump()));
            return $"[ {choices} ]";
        }

        public bool MayBeEmpty()
        {
            return true;
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return ToString();
        }

        public bool Equals(IClause<T> clause)
        {
            if (clause is ChoiceClause<T> other)
            {
                return Equals(other);
            }

            return false;
        }

        private bool Equals(ChoiceClause<T> other)
        {
            if (other.Choices.Count != Choices.Count)
            {
                return false;
            }

            if (other.IsTerminalChoice != IsTerminalChoice)
            {
                return false;
            }

            return other.Choices.TrueForAll(x => Choices.Exists(y => y.Equals(x)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChoiceClause<T>)obj);
        }

        public override int GetHashCode()
        {
            return Dump().GetHashCode();
        }

    }
}