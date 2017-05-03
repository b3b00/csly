using sly.parser;

namespace sly.lexer
{
    public class LexicalError : ParseError
    {

        public char UnexpectedChar {get; set;}

        public override string ErrorMessage { get
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
