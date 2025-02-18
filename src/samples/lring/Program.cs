namespace lr1;

class Program
{
    static void Main()
    {
        string grammar = "E ::= E + T | T\nT ::= T * F | F\nF ::= ( E ) | id";
        var parser = new BNFParser();
        var rules = parser.ParseGrammar(grammar);
        var (actionTable, gotoTable) = LR1TableBuilder.BuildLR1Table(rules);
        var lr1Parser = new LR1Parser(actionTable, gotoTable);
        List<string> input = new List<string> { "id", "+", "id", "*", "id" };
        var parseTree = lr1Parser.Parse(input);
        Console.WriteLine(parseTree != null ? "Accepted" : "Rejected");
    }
}