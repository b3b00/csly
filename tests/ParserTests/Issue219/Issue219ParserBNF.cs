using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue219;

public class Issue219ParserBNF
{
    [Production("root: set")]
    public I219Ast root(I219Ast set)
    {
        return new Root219() {Sets = new List<I219Ast>(){set}};
    }

    [Production("set : ID EQ INT")]
    public Set219 set(Token<Issue219Lexer> id, Token<Issue219Lexer> eq, Token<Issue219Lexer> value)
    {
        throw new Exception219("visitor error");
    }
}