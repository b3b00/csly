using sly.i18n;
using sly.parser;

namespace sly.lexer
{
    public class IndentationError : LexicalError
    {
        public IndentationError(int line, int column, string i18n) : base(line,column,' ',i18n)
        {
            Line = line;
            Column = column;
            ErrorType = ErrorType.IndentationError;

        }

        public override string ErrorMessage =>
            $"Indentation error at  (line {Line}, column {Column}).";
        
        protected override string GetContextualMessage(string fullSource)
        {
            var message = I18N.Instance.GetText(_i18N, I18NMessage.IndentError);
            return GetContextualMessage(fullSource, Line, Column, message);
        }

    }
}