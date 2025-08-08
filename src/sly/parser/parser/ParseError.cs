using System.Text;

namespace sly.parser
{

    public enum ErrorType
    {
        UnexpectedEOS,
        UnexpectedToken,
        UnexpectedChar,
        IndentationError
    }
    
    public abstract class ParseError
    {
        public virtual ErrorType ErrorType { get; protected set; }
        public virtual int Column { get; protected set; }
        public virtual string ErrorMessage { get; protected set; }
        public virtual int Line { get; protected set; }

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

        public virtual string GetContextualMessage(string fullSource) => ErrorMessage;
        
        
        protected string GetContextualMessage(string fullSource, int line, int column, string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ErrorMessage);
            var theLine = fullSource.GetLines()[line];
            var tab = " ".Multiply(line.ToString().Length);
            sb.Append(tab).AppendLine(" |");
            sb.Append(line).Append(" |").AppendLine(theLine);
            sb.Append($"{tab} |").Append(" ".Multiply(column)).Append("^^^").AppendLine($" {message}");
            
            return sb.ToString();
        }
    }
}