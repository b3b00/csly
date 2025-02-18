using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue277;

public class Issue277Parser
{
    [Production("widget: IDENTIFIER")]
    public string Widget(Token<Issue277Tokens> widget)
    {
        return widget.Value;
    }

    [Production("expression: widget (OR [d] widget)+")]
    public string Expression(string widget, List<Group<Issue277Tokens, string>> ors)
    {
        return ors.Aggregate($"{widget}", (acc, a) => $"{acc} | {a.Value("widget")}");
    }
}