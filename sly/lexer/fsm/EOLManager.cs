using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class EOLManager
    {
        public static string GetToEndOfLine(string value, int position)
        {
            var CurrentPosition = position;
            var current = value[CurrentPosition];
            var end = IsEndOfLine(value, CurrentPosition);
            while (CurrentPosition < value.Length && end == EOLType.No)
            {
                CurrentPosition++;
                end = IsEndOfLine(value, CurrentPosition);
            }

            return value.Substring(position, CurrentPosition - position + (end == EOLType.Windows ? 2 : 1));
        }

        public static EOLType IsEndOfLine(string value, int position)
        {
            var end = EOLType.No;
            var n = value[position];
            if (n == '\n')
            {
                end = EOLType.Nix;
            }
            else if (n == '\r')
            {
                if (value[position + 1] == '\n')
                    end = EOLType.Windows;
                else
                    end = EOLType.Mac;
            }

            return end;
        }

        public static List<string> GetLines(string value)
        {
            var lines = new List<string>();
            var previousStart = 0;
            var i = 0;
            while (i < value.Length)
            {
                var end = IsEndOfLine(value, i);
                if (end != EOLType.No)
                {
                    if (end == EOLType.Windows) i++;
                    var line = value.Substring(previousStart, i - previousStart);
                    lines.Add(line);
                    previousStart = i + 1;
                }

                i++;
            }

            lines.Add(value.Substring(previousStart, i - previousStart));
            return lines;
        }
    }
}