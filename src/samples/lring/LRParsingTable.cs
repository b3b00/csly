namespace lr1;

public class LRParsingTable<T> where T : Enum
{
    private Dictionary<(int, T), string> ActionTable;
    private Dictionary<(int, T), int> GotoTable;

    public LRParsingTable()
    {
        ActionTable = new Dictionary<(int, T), string>();
        GotoTable = new Dictionary<(int, T), int>();
    }

    public void AddAction(int state, T token, string action)
    {
        ActionTable[(state, token)] = action;
    }

    public void AddGoto(int state, T nonTerminal, int nextState)
    {
        GotoTable[(state, nonTerminal)] = nextState;
    }

    public string GetAction(int state, T token)
    {
        return ActionTable.TryGetValue((state, token), out var action) ? action : "ERROR";
    }

    public int GetGoto(int state, T nonTerminal)
    {
        return GotoTable.TryGetValue((state, nonTerminal), out var nextState) ? nextState : -1;
    }

    public void PrintTable()
    {
        Console.WriteLine("Action Table:");
        foreach (var entry in ActionTable)
            Console.WriteLine($"State {entry.Key.Item1}, Token {entry.Key.Item2} → {entry.Value}");

        Console.WriteLine("\nGoto Table:");
        foreach (var entry in GotoTable)
            Console.WriteLine($"State {entry.Key.Item1}, Non-Terminal {entry.Key.Item2} → {entry.Value}");
    }
}