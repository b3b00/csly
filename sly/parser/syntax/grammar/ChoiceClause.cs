using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.parser.syntax.grammar
{
    public class ChoiceClause<IN,OUT> : IClause<IN,OUT>
    {

        public bool IsDiscarded { get; set; } = false;
        public bool IsTerminalChoice => Choices.Select(c => c is TerminalClause<IN,OUT>).Aggregate((x, y) => x && y);
        public bool IsNonTerminalChoice => Choices.Select(c => c is NonTerminalClause<IN,OUT>).Aggregate((x, y) => x && y);
            
        public  List<IClause<IN,OUT>> Choices { get; set; }
        public ChoiceClause(IClause<IN,OUT> clause)
        {
            Choices = new List<IClause<IN,OUT>>() {clause};
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
            return string.Join(" | ", Choices.Select(c => c.ToString()));
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
    }
}