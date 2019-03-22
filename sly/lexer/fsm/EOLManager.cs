using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class EOLManager
    {
        public static ReadOnlyMemory<char> GetToEndOfLine(ReadOnlyMemory<char> value, int position)
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

        public static EOLType IsEndOfLine(ReadOnlyMemory<char> value, int position)
        {
            var end = EOLType.No;
            var n = value.At(position);
            if (n == '\n')
            {
                end = EOLType.Nix;
            }
            else if (n == '\r')
            {
                if (value.At(position + 1) == '\n')
                    end = EOLType.Windows;
                else
                    end = EOLType.Mac;
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