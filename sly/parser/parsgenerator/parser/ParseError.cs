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
            int comparison = 0;
            ParseError unexpectedError = obj as ParseError;
            if (unexpectedError != null)
            {
                int lineComparison = Line.CompareTo(unexpectedError.Line);
                int columnComparison = Column.CompareTo(unexpectedError.Column);

                if (lineComparison > 0)
                {
                    comparison = 1;
                }
                if (lineComparison == 0)
                {
                    comparison = columnComparison;
                }
                if (lineComparison < 0)
                {
                    comparison = -1;
                }
            }
            return comparison;
        }
    }
}