namespace sly.parser
{
    public class ParseError
    {
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
            var unexpectedError = obj as ParseError;
            if (unexpectedError != null)
            {
                var lineComparison = Line.CompareTo(unexpectedError != null ? unexpectedError.Line : 0);
                var columnComparison = Column.CompareTo(unexpectedError != null ? unexpectedError.Column : 0);

                if (lineComparison > 0) comparison = 1;
                if (lineComparison == 0) comparison = columnComparison;
                if (lineComparison < 0) comparison = -1;
            }

            return comparison;
        }
    }
}