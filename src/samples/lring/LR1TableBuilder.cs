namespace sly.parser.bnf;

public class LR1TableBuilder
{
    public static (Dictionary<(int, string), string>, Dictionary<(int, string), int>) BuildLR1Table(Dictionary<string, List<List<string>>> grammar)
    {
        var actionTable = new Dictionary<(int, string), string>();
        var gotoTable = new Dictionary<(int, string), int>();

        int stateCounter = 0;
        var stateMapping = new Dictionary<string, int>();
            
        foreach (var rule in grammar)
        {
            if (!stateMapping.ContainsKey(rule.Key))
            {
                stateMapping[rule.Key] = stateCounter++;
            }

            foreach (var production in rule.Value)
            {
                foreach (var symbol in production)
                {
                    if (!stateMapping.ContainsKey(symbol))
                    {
                        stateMapping[symbol] = stateCounter++;
                    }
                }
            }
        }

        foreach (var rule in grammar)
        {
            int state = stateMapping[rule.Key];
            foreach (var production in rule.Value)
            {
                if (production.Count == 1 && grammar.ContainsKey(production[0]))
                {
                    gotoTable[(state, production[0])] = stateMapping[production[0]];
                }
                else
                {
                    actionTable[(state, production[0])] = "Shift " + stateMapping[production[0]];
                }
            }
        }

        return (actionTable, gotoTable);
    }
}