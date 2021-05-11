using System;
using System.Collections.Immutable;




namespace sly.lexer

{
    public class LexerPosition : IComparable
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

        public int CompareTo(object obj)
        {
            if (obj != null)
            {
                if (obj is LexerPosition position)
                {
                    if (Line < position.Line)
                    {
                        return -1;
                    }
                    if (Line == position.Line)
                    {
                        return Column.CompareTo(position.Column);
                    }
                }
            }

            return 1;
        }

        public LexerPosition Clone()
        {
            return new LexerPosition(Index,Line,Column,Indentations, CurrentIndentation);
        }
    }
    

}