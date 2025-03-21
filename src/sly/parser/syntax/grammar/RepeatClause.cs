using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class RepeatClause<IN,OUT> : ManyClause<IN,OUT> where IN : struct
    {
        public int MinRepetitionCount { get; set; }
        
        public int MaxRepetitionCount { get; set; }
        
        bool IsRangeRepetition => MaxRepetitionCount !=  MinRepetitionCount;
        public RepeatClause(IClause<IN,OUT> clause, int minNumber, int maxNumber)
        {
            Clause = clause;
            MinRepetitionCount = minNumber; 
            MaxRepetitionCount = maxNumber;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
           
                return $"{Clause}{DumpRange()}";
           
        }

        public string DumpRange()
        {
            if (IsRangeRepetition)
            {
                return $"{{{MinRepetitionCount}-{MaxRepetitionCount}}}";
            }

            return $"{{{MinRepetitionCount}}}";
        }



        public override bool MayBeEmpty()
        {
            return MinRepetitionCount == 0;
        }
        
        [ExcludeFromCodeCoverage]
        public override string Dump() => ToString();

        public override bool Equals(IClause<IN,OUT> other)
        {
            if (other is RepeatClause<IN,OUT> RepeatClause)
            {
                return Clause.Equals(RepeatClause.Clause)  && MinRepetitionCount == RepeatClause.MinRepetitionCount && MaxRepetitionCount == RepeatClause.MaxRepetitionCount;
            }
            return false;
        }
    }
}