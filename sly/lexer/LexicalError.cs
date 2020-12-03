using sly.parser;

namespace sly.lexer
{
    public class LexicalError : ParseError
    {

        public LexicalError(int line, int column, string errorMessage)
        {
            Line = line;
            Column = column;
            ErrorMessage = errorMessage;
        }
        
        public LexicalError(int line, int column, char unexpectedChar)
        {
            Line = line;
            Column = column;
            UnexpectedChar = unexpectedChar;
            ErrorMessage = $"Lexical Error : Unrecognized symbol '{UnexpectedChar}' at  (line {Line}, column {Column}).";
        }

        public char UnexpectedChar { get; set; }

        public override string ErrorMessage { get; set; }
    
            

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}