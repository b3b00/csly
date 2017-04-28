namespace cpg.parser.parsgenerator.parser
{
    public class ParseError
    {
        public virtual int Column { get; }
        public virtual string ErrorMessage { get; set; }
        public virtual int Line { get; }

        

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