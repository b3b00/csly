namespace sly.lexer
{
    public class TokenPosition
    {
        public TokenPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public int Column { get; }
        public int Index { get; }
        public int Line { get; }

        public override string ToString()
        {
            return $"line {Line}, column {Column}";
        }
    }
}