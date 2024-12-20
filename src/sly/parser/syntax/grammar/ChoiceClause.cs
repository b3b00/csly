using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public sealed class ChoiceClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {

        public bool IsDiscarded { get; set; } = false;
        public bool IsTerminalChoice => Choices.Select(c => c is TerminalClause<IN,OUT>).Aggregate((x, y) => x && y);
        public bool IsNonTerminalChoice => Choices.Select(c => c is NonTerminalClause<IN,OUT>).Aggregate((x, y) => x && y);
            
        public  List<IClause<IN,OUT>> Choices { get; }
        public ChoiceClause(IClause<IN,OUT> clause)
        {
            Choices = new List<IClause<IN,OUT>> {clause};
        }
        
        public ChoiceClause(List<IClause<IN,OUT>> choices)
        {
            Choices = choices;
        }
        
        public ChoiceClause(IClause<IN,OUT> choice, List<IClause<IN,OUT>> choices) : this(choice)
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

        public bool Equals(IClause<IN,OUT> clause)
        {
            if (clause is ChoiceClause<IN,OUT> other)
            {
                return Equals(other);
            }

            return false;
        }

        private bool Equals(ChoiceClause<IN,OUT> other)
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
            return Equals((ChoiceClause<IN,OUT>)obj);
        }

        public override int GetHashCode()
        {
            return Dump().GetHashCode();
        }

    }
}