using System;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class DateExpression : ValueExpression
    {
        public int OffsetMagnitude { get; }
        public DateOffsetKind OffsetKindKind { get; }
        public DateRoundKind DateRoundKind { get; }
        public TimeSpan Offset => TimeSpan.FromSeconds(OffsetMagnitude * (int)OffsetKindKind);

        public DateExpression(string value,
            int offsetMagnitude,
            DateOffsetKind offsetKindKind, 
            DateRoundKind dateRoundKind)
            : base($"{value}{(offsetMagnitude != 0? offsetMagnitude.ToString() : "")}{(offsetMagnitude != 0? offsetKindKind + (Math.Abs(offsetMagnitude) > 1? "S" : "") : "")}{(dateRoundKind != DateRoundKind.NONE? "/" + dateRoundKind : "")}")
        {
            OffsetMagnitude = offsetMagnitude;
            OffsetKindKind = offsetKindKind;
            DateRoundKind = dateRoundKind;
        }

        public override string ToQuery()
        {
            if (Value.Contains(' ')
                || Value.Contains('\'')
                || Value.Contains('"')
                || Value.Contains(':'))
            {
                var escapedValue = Value.Replace(@"\""", @"""").Replace(@"\'", "'").Replace("\"", "\\\"");
                return $"\"{escapedValue}\"";
            }
            
            return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
