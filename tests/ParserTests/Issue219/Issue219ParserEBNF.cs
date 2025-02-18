using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue219;

public class Issue219ParserEBNF
{
    [Production("root: set*")]
    public I219Ast root(List<I219Ast> sets)
    {
        return new Root219() {Sets = sets};
    }

    [Production("set : ID EQ[d] INT")]
    public Set219 set(Token<Issue219Lexer> id, Token<Issue219Lexer> value)
    {
        throw new Exception219("visitor error");
    }
}