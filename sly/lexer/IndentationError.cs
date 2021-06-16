using sly.parser;

namespace sly.lexer
{
    public class IndentationError : LexicalError
    {
        public IndentationError(int line, int column, string i18n) : base(column,line,' ',i18n)
        {
            Line = line;
            Column = column;
            ErrorType = ErrorType.IndentationError;

        }

        public override string ErrorMessage =>
            $"Indentation error at  (line {Line}, column {Column}).";

    }
}