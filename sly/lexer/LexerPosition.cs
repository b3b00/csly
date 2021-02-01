using System.Collections.Immutable;

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
            Indentations = ImmutableStack<string>.Empty;
        }
        
        public LexerPosition(int index, int line, int column, int currentIndentation) : this(index, line, column)
        {
            CurrentIndentation = currentIndentation;
            ;
           
        }
        
        public LexerPosition(int index, int line, int column, ImmutableStack<string> indentations) : this(index, line, column)
        {
            Indentations = indentations;
        }
        
        public LexerPosition(int index, int line, int column, ImmutableStack<string> indentations, int currentIndentation) : this(index, line, column, indentations)
        {
            CurrentIndentation = currentIndentation;
        }

        public bool IsStartOfLine => Column == 0;


        public ImmutableStack<string> Indentations { get; set; }
        public int CurrentIndentation { get; set; }

        public int Column { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }

        public override string ToString()
        {
            return $"line {Line}, column {Column}";
        }

        public LexerPosition Clone()
        {
            return new LexerPosition(Index,Line,Column,Indentations, CurrentIndentation);
        }
    }
    

}