using System.Text.RegularExpressions;

namespace sly.parser.bnf;

public class BNFParser
{
    private static readonly Regex RuleRegex = new Regex(@"^(\w+)\s*::=\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex TokenRegex = new Regex(@"\w+|[+*()|]", RegexOptions.Compiled);

    public Dictionary<string, List<List<string>>> ParseGrammar(string bnfGrammar)
    {
        var rules = new Dictionary<string, List<List<string>>>();
        var lines = bnfGrammar.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
        foreach (var line in lines)
        {
            var match = RuleRegex.Match(line.Trim());
            if (match.Success)
            {
                string nonTerminal = match.Groups[1].Value;
                string expression = match.Groups[2].Value;

                if (!rules.ContainsKey(nonTerminal))
                {
                    rules[nonTerminal] = new List<List<string>>();
                }

                var alternatives = expression.Split('|');
                foreach (var alt in alternatives)
                {
                    var tokens = TokenRegex.Matches(alt.Trim());
                    var ruleTokens = new List<string>();
                    foreach (Match token in tokens)
                    {
                        ruleTokens.Add(token.Value);
                    }
                    rules[nonTerminal].Add(ruleTokens);
                }
            }
            else
            {
                throw new Exception($"Invalid BNF syntax: {line}");
            }
        }

        return rules;
    }
}