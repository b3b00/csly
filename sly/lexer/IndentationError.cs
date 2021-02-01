using sly.parser;

namespace sly.lexer
{
    public class IndentationError : LexicalError
    {
        public IndentationError(int line, int column) : base(column,line,' ')
        {
            Line = line;
            Column = column;
        }

        public char UnexpectedChar { get; set; }

        public override string ErrorMessage =>
            $"Indentation error at  (line {Line}, column {Column}).";

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}