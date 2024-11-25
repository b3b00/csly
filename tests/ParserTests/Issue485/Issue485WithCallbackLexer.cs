using System;
using sly.lexer;

namespace ParserTests.Issue485;

[CallBacks(typeof(Issue485Callbacks))]
public enum Issue485WithCallbackLexer
{
    [Sugar(":")] COLON,
    [Keyword("Property")] PROPERTY, 
    [String] STRING
}

public class Issue485Callbacks
{

    [TokenCallback((int)Issue485WithCallbackLexer.STRING)]
    public static Token<Issue485WithCallbackLexer> EscapeEscapedSequences(Token<Issue485WithCallbackLexer> token)
    {
        string value = token.Value;
        
        value = value.Replace("\\n", "\n");
        value = value.Replace("\\r", "\r");
        value = value.Replace("\\t", "\t");
        value = value.Replace("\\\\", "\\"); 
        token.SpanValue = new ReadOnlyMemory<char>(value.ToCharArray());
        return token;
    }
}