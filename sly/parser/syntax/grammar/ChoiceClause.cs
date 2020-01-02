using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public class ChoiceClause<T> : IClause<T>
    {
        public  List<IClause<T>> Choices { get; set; }
        public ChoiceClause(IClause<T> clause)
        {
            Choices = new List<IClause<T>>() {clause};
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
            return string.Join(" | ", Choices.Select(c => c.ToString()));
        }

        public bool MayBeEmpty()
        {
            return false;
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return ToString();
        }
    }
}