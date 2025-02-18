namespace sly.parser.bnf;

public class LR1Parser
{
    private Dictionary<(int, string), string> ActionTable;
    private Dictionary<(int, string), int> GotoTable;
    private Stack<int> StateStack;
    private Stack<LR1ParserNode> SymbolStack;

    public LR1Parser(Dictionary<(int, string), string> actionTable, Dictionary<(int, string), int> gotoTable)
    {
        ActionTable = actionTable;
        GotoTable = gotoTable;
        StateStack = new Stack<int>();
        SymbolStack = new Stack<LR1ParserNode>();
    }

    public LR1ParserNode Parse(List<string> tokens)
    {
        StateStack.Push(0);
        tokens.Add("$"); // End of input marker
        int position = 0;

        while (position < tokens.Count)
        {
            string currentToken = tokens[position];
            int currentState = StateStack.Peek();

            if (ActionTable.TryGetValue((currentState, currentToken), out var action))
            {
                if (action.StartsWith("Shift"))
                {
                    int nextState = int.Parse(action.Split(' ')[1]);
                    Shift(currentToken, nextState);
                    position++;
                }
                else if (action.StartsWith("Reduce"))
                {
                    var parts = action.Split(' ');
                    int popCount = int.Parse(parts[2]);
                    string nonTerminal = parts[1];
                    Reduce(nonTerminal, popCount);
                }
                else if (action == "Accept")
                {
                    return SymbolStack.Peek();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    private void Shift(string token, int nextState)
    {
        SymbolStack.Push(new LR1ParserNode(token));
        StateStack.Push(nextState);
    }

    private void Reduce(string nonTerminal, int count)
    {
        var node = new LR1ParserNode(nonTerminal);
        var children = new List<LR1ParserNode>();
        for (int i = 0; i < count; i++)
        {
            children.Insert(0, SymbolStack.Pop());
            StateStack.Pop();
        }
        node.Children.AddRange(children);
        SymbolStack.Push(node);
        int currentState = StateStack.Peek();
        if (GotoTable.TryGetValue((currentState, nonTerminal), out int nextState))
        {
            StateStack.Push(nextState);
        }
    }
}