namespace sly.parser
{

    public enum ErrorType
    {
        UnexpectedEOS,
        UnexpectedToken,
        UnexpectedChar,
        IndentationError
    }
    
    public class ParseError
    {
        public virtual ErrorType ErrorType { get; protected set; }
        public virtual int Column { get; protected set; }
        public virtual string ErrorMessage { get; protected set; }
        public virtual int Line { get; protected set; }


        //public ParseError(int line, int column)
        //{
        //    Column = column;
        //    Line = line;
        //}

        public int CompareTo(object obj)
        {
            var comparison = 0;
            if (obj is ParseError unexpectedError)
            {
                var lineComparison = Line.CompareTo(unexpectedError.Line);
                var columnComparison = Column.CompareTo(unexpectedError.Column);

                if (lineComparison > 0) comparison = 1;
                if (lineComparison == 0) comparison = columnComparison;
                if (lineComparison < 0) comparison = -1;
            }

            return comparison;
        }
    }
}