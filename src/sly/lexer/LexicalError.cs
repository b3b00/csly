using System.Diagnostics.CodeAnalysis;
using sly.i18n;
using sly.parser;

namespace sly.lexer
{
    public class LexicalError : ParseError
    {
        public LexicalError(int line, int column, char unexpectedChar, string i18n)
        {
            Line = line;
            Column = column;
            UnexpectedChar = unexpectedChar;
            ErrorType = ErrorType.UnexpectedChar;
            _i18N = i18n;
        }

        protected readonly string _i18N;

        public char UnexpectedChar { get; set; }

        public override string ErrorMessage => I18N.Instance.GetText(_i18N, I18NMessage.UnexpectedChar,
            UnexpectedChar.ToString(), Line.ToString(), Column.ToString(), ((int)UnexpectedChar).ToString());

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return ErrorMessage;
        }

        protected override string GetContextualMessage(string fullSource)
        {
            var message = I18N.Instance.GetText(_i18N, I18NMessage.UnexpectedCharacter,
                UnexpectedChar.ToString());
            return GetContextualMessage(fullSource, Line, Column, message);
        }
    }
}