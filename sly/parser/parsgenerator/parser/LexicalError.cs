using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.parser
{
    public class LexicalError : ParseError
    {

        public char UnexpectedChar {get; set;}

        public string ErrorMessage { get
            {
                return $"Lexical Error : Unrecognized symbol '{UnexpectedChar}' at  (line {Line}, column {Column}).";
            }
        }

        public LexicalError(int line, int column, char unexpectedChar)
        {
            this.Line = line;
            this.Column = column;
            this.UnexpectedChar = unexpectedChar;
        }



    }
}
