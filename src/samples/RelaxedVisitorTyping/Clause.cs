namespace RelaxedVisitorTyping;

public class Clause
{
    public string Property { get; set; }
    
    public string Op { get; set; }
    
    public object Value { get; set; }

public string Operator()  {
    string op = Op;
    if (op.StartsWith("eq"))
    {
        return "==";
    }

    if (op.StartsWith("ne") || op.StartsWith("not"))
    {
        return "<>";
    }

    if (op.StartsWith("lt") || op.StartsWith("lower"))
    {
        return "<";
    }

    if (op.StartsWith("gt") || op.StartsWith("greater"))
    {
        return ">";
    }
    return op;
}
    
    public override string ToString() => $"{Property} {Operator()} {Value}";
}