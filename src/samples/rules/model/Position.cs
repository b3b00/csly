namespace rules.model;

public class Position
{
    public Position() : this(0, 0)
    {
    }

    public Position(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public int Column { get; set; }
    public int Line { get; set; }

    public override string ToString()
    {
        return $"line {Line + 1}, column {Column + 1}";
    }
}
