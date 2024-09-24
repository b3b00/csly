using System.Text;
using handExpressions.ebnfparser.model;

namespace handExpressions.extractor;

public class RuleGenerator : IParserModelVisitor<string>
{
    private readonly string _lexerType;
    private readonly string _outputType;

    public RuleGenerator(string lexerType, string outputType)
    {
        _lexerType = lexerType;
        _outputType = outputType;
    }
    public string VisitTerminal(TerminalClause terminal)
    {
        // [Discarded]TerminalParser(expectedTokens:GenericExpressionToken.LPAREN);
        var result = $"{(terminal.IsDiscared ? "DiscardedTerminalParser" : "TerminalParser")}(expectedTokens:{_lexerType}.{terminal.Terminal})";
        return result;
    }

    public string VisitNonTerminal(NonTerminalClause nonTerminal)
    {
        return "_"+nonTerminal.NonTerminal;
    }

    public string VisitOption(OptionalClause option, string clause)
    {
        var method = option.Clause switch
        {
            TerminalClause terminal => "ZeroOrMoreToken",
            NonTerminalClause nonTerminal => "ZeroOrMoreValue",
            GroupClause groupClause => "ZeroOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitZeroOrMore(ZeroOrMoreClause zeroOrMore, string clause)
    {
        var method = zeroOrMore.Clause switch
        {
            TerminalClause terminal => "ZeroOrMoreToken",
            NonTerminalClause nonTerminal => "ZeroOrMoreValue",
            GroupClause groupClause => "ZeroOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitOneOrMore(OneOrMoreClause oneOrMore, string clause)
    {
        var method = oneOrMore.Clause switch
        {
            TerminalClause terminal => "OneOrMoreToken",
            NonTerminalClause nonTerminal => "OneOrMoreValue",
            GroupClause groupClause => "OneOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitGroup(GroupClause group, IList<string> clauses)
    {
        return $"SubGroup({string.Join(", ", clauses)})";
    }
  

    public string VisitAlternate(AlternateClause alternate, IList<string> clauses)
    {
        return $"Alternate({string.Join(", ", clauses)})";
    }
    
    public string VisitRule(Rule rule, IList<string> clauses)
    {
        StringBuilder builder = new StringBuilder();
        
        var methodName = rule.Number >= 0 ? $"_{rule.NonTerminalName}_{rule.Number}" : $"_{rule.NonTerminalName}";
        
        
        builder.AppendLine(
            $"    public Match<{_lexerType},{_outputType}> {methodName}(IList<Token<{_lexerType}>> tokens, int position) {{");
        builder.AppendLine($"        var parser = Sequence({string.Join(", ", clauses)});");
        builder.AppendLine($"        var result = parser(tokens,position);");
        builder.AppendLine("        return result;");
        builder.AppendLine("    }");
        return builder.ToString();
    }
}