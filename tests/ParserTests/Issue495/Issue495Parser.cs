using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue495;

public class Issue495Parser
{

    public Issue495Parser()
    {
        
    }

    [Production("STRING: StartQuote StringValue* EndQuote")]
    public string stringValue(Token<Issue495Token> open, List<Token<Issue495Token>> values, Token<Issue495Token> close)
    {
        return string.Join(", ", values.Select(x => x.Value.ToString()));
    }

    [Production("statement : Identifier Assign STRING End")]
    [Production("statement : '$'[d] Identifier Assign STRING End")]
    public string Statement(Token<Issue495Token> id, Token<Issue495Token> assign, string value,
        Token<Issue495Token> end)
    {
        return $"{id.Value}{assign.Value}{value}";
    }

    [Production("program: statement*")]
    public string Program(List<string> statements)
    {
        return string.Join("\n", statements);
    }
}