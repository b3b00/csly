using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer.fsm
{

    public class EOLManager
    {


        public static string GetToEndOfLine(string value, int position)
        {
            int CurrentPosition = position;
            char current = value[CurrentPosition];
            EOLType end = IsEndofLine(value, CurrentPosition);
            while (CurrentPosition < value.Length && end == EOLType.No)
            {
                CurrentPosition++;
                end = IsEndofLine(value, CurrentPosition);
            }
            return value.Substring(position, CurrentPosition - position + (end == EOLType.Windows ? 2 : 1));
        }

        public static EOLType IsEndofLine(string value, int position)
        {
            EOLType end = EOLType.No;
            char n = value[position];
            if (n == '\n')
            {
                end = EOLType.Nix;
            }
            else if (n == '\r')
            {
                if (value[position + 1] == '\n')
                {
                    end = EOLType.Windows;
                }
                else
                {
                    end = EOLType.Mac;
                }
            }
            return end;
        }

        public static List<string> GetLines(string value)
        {
            var lines = new List<string>();
            int previousStart = 0;
            int i = 0;
            while (i < value.Length)
            {
                var end = IsEndofLine(value, i);
                if (end != EOLType.No)
                {
                    if (end == EOLType.Windows)
                    {
                        i++;
                    }
                    string line = value.Substring(previousStart, i - previousStart);
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