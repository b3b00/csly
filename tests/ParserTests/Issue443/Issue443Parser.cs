using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue443;

public class Issue443Parser
{
    [Production("template: eol*")]
    public INode Template(List<INode> items)
    {
        return new TemplateNode(items);
    }

    [Production("eol : [CRLF | LF]")]
    public INode EndOfLine(Token<Issue443Lexer> eol) {
        return new Item(eol.Value);
    }
}