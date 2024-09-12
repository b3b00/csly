using System;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorDateExpression : Issue223OorValueExpression
    {
        public int OffsetMagnitude { get; }
        public Issue223OorDateOffsetKind OffsetKindKind { get; }
        public Issue223OorDateRoundKind Issue223OorDateRoundKind { get; }
        public TimeSpan Offset => TimeSpan.FromSeconds(OffsetMagnitude * (int)OffsetKindKind);

        public Issue223OorDateExpression(string value,
            int offsetMagnitude,
            Issue223OorDateOffsetKind offsetKindKind, 
            Issue223OorDateRoundKind issue223OorDateRoundKind)
            : base($"{value}{(offsetMagnitude != 0? offsetMagnitude.ToString() : "")}{(offsetMagnitude != 0? offsetKindKind + (Math.Abs(offsetMagnitude) > 1? "S" : "") : "")}{(issue223OorDateRoundKind != Issue223OorDateRoundKind.NONE? "/" + issue223OorDateRoundKind : "")}")
        {
            OffsetMagnitude = offsetMagnitude;
            OffsetKindKind = offsetKindKind;
            Issue223OorDateRoundKind = issue223OorDateRoundKind;
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
