using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

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
            Mode = mode;
        }
        
        private LexerPosition(int index, int line, int column, int currentIndentation, string mode = ModeAttribute.DefaultLexerMode) : this(index, line, column, mode)
        {
            CurrentIndentation = currentIndentation;
        }

        public bool IsStartOfLine => Column == 0;

        public int CurrentIndentation { get; set; }

        [JsonIgnore]
        public LexerIndentation Indentation { get; set; } = new LexerIndentation();

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
            if (obj is not LexerPosition position) return 1;
            
            if (Line < position.Line)
            {
                return -1;
            }
            if (Line == position.Line)
            {
                return Column.CompareTo(position.Column);
            }

            return 1;
        }

        public LexerPosition Clone()
        {
            return new LexerPosition(Index, Line, Column, CurrentIndentation)
            {
                Indentation = this.Indentation.Clone(),
                Mode = Mode
            };
        }

        public static bool operator ==(LexerPosition p1, LexerPosition p2)
        {
            return p1.Index == p2.Index;
        }
        
        public static bool operator !=(LexerPosition p1, LexerPosition p2)
        {
            return p1.Index != p2.Index;
        }
    }
    

}
