namespace sly.lexer
{
    public class LexerPosition
    {

        public LexerPosition() : this (0,0,0)
        {
        }
        
        public LexerPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public int Column { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }

        public override string ToString()
        {
            return $"line {Line}, column {Column}";
        }

        public LexerPosition Clone()
        {
            return new LexerPosition(Index,Line,Column);
        }
    }
}