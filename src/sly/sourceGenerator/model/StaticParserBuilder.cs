using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.sourceGenerator.model;

public class StaticParserBuilder
{
    public List<string> _tokens { get; set; }

    public List<Rule> Model { get; set; } = new List<Rule>();

    public StaticParserBuilder(List<string> tokens)
    {
        _tokens = tokens;
    }

    public Rule Parse(string ruleString)
    {
            (string head, string[] clauses) result = (null,null);
            if (ruleString != null)
            {
                ruleString = ruleString.Substring(1, ruleString.Length - 2);
                var i = ruleString.IndexOf(":", StringComparison.Ordinal);
                if (i <= 0)
                {
                    return null;
                }

                var head = ruleString.Substring(0, i).Trim();
                var clauseStrings = ruleString.Substring(i + 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var clauses = clauseStrings.Select(
                    x => _tokens.Contains(x) ? 
                        (IClause)new TerminalClause(x) :
                        (IClause)new NonTerminalClause(x)).ToList();
                var rule = new Rule(head, clauses);
                Model.Add(rule);
                return rule;
            }
        return null;
    }
}