using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace sly.lexer

{
    public class LexerPosition : IComparable
    {

        public LexerPosition() : this (0,0,0)
        {
        }
        
        public LexerPosition(int index, int line, int column, string mode = ModeAttribute.DefaultLexerMode)
        {
            Index = index;
            Line = line;
            Column = column;
            Indentations = ImmutableStack<string>.Empty;
            Mode = mode;
        }
        
        public LexerPosition(int index, int line, int column, ImmutableStack<string> indentations, string mode = ModeAttribute.DefaultLexerMode) : this(index, line, column, mode)
        {
            Indentations = indentations;
        }
        
        public LexerPosition(int index, int line, int column, ImmutableStack<string> indentations, int currentIndentation, string mode = ModeAttribute.DefaultLexerMode) : this(index, line, column, indentations, mode)
        {
            CurrentIndentation = currentIndentation;
        }

        public bool IsStartOfLine => Column == 0;


        public ImmutableStack<string> Indentations { get; set; }
        public int CurrentIndentation { get; set; }

        public int Column { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }
        
        public string Mode { get; set; }

        public override string ToString()
        {
            return $"line {Line}, column {Column}";
        }

        [ExcludeFromCodeCoverage]
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
            return new LexerPosition(Index, Line, Column, Indentations, CurrentIndentation)
            {
                Mode = Mode
            };
        }
    }
    

}
