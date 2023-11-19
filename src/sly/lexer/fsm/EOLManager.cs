using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer.fsm
{
    public static class EOLManager
    {
        public static ReadOnlyMemory<char> GetToEndOfLine(this ReadOnlyMemory<char> value, int position)
        {
            var CurrentPosition = position;
            var spanValue = value.Span;
            var current = spanValue[CurrentPosition];
            var end = IsEndOfLine(value, CurrentPosition);
            while (CurrentPosition < value.Length && end == EOLType.No)
            {
                CurrentPosition++;
                end = IsEndOfLine(value, CurrentPosition);
            }

            return value.Slice(position, CurrentPosition - position + (end == EOLType.Windows ? 2 : 1));
        }

        public static ReadOnlyMemory<char> RemoveEndOfLineChars(this ReadOnlyMemory<char> value)
        {
            int endPosition = value.Length-1;
            while (new[] { '\n', '\r' }.Contains(value.At(endPosition)))
            {
                endPosition--;
            }
            return value.Slice(0,endPosition+1);
        }

        public static EOLType IsEndOfLine(ReadOnlyMemory<char> value, int position)
        {
            var end = EOLType.No;
            var n = value.At<char>(position);
            switch (n)
            {
                case '\n':
                    end = EOLType.Nix;
                    break;
                case '\r' when value.At<char>(position + 1) == '\n':
                    end = EOLType.Windows;
                    break;
                case '\r':
                    end = EOLType.Mac;
                    break;
            }

            return end;
        }

        public static List<int> GetLinesLength(ReadOnlyMemory<char> value)
        {
            var lineLengths = new List<int>();
            var lines = new List<string>();
            var previousStart = 0;
            var i = 0;
            while (i < value.Length)
            {
                var end = IsEndOfLine(value, i);
                if (end != EOLType.No)
                {
                    if (end == EOLType.Windows) i ++;
                    var line = value.Slice(previousStart, i - previousStart);
                    lineLengths.Add(line.Length);
                    lines.Add(line.ToString());
                    previousStart = i + 1;
                }

                i++;
            }

            lineLengths.Add(value.Slice(previousStart, i - previousStart).Length);
            return lineLengths;
        }
    }
}