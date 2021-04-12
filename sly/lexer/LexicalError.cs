using sly.i18n;
using sly.parser;

namespace sly.lexer
{
    public class LexicalError : ParseError
    {
        public LexicalError(int line, int column, char unexpectedChar)
        {
            Line = line;
            Column = column;
            UnexpectedChar = unexpectedChar;
            ErrorType = ErrorType.UnexpectedChar;
        }

        public char UnexpectedChar { get; set; }

        public override string ErrorMessage => I18N.Instance.GetText(Message.UnexpectedChar,UnexpectedChar.ToString());

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}