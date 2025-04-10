using sly.lexer;
using sly.parser.generator;

namespace ParserTests.dateIssue;


    [ParserRoot("root")]
    public class Issue554Parser
    {
        [Production("root : DOUBLE INT DATE")]
        public object root_DOUBLE_INT_DATE(Token<Issue554lexer> p0, Token<Issue554lexer> p1, Token<Issue554lexer> p2)
        {
            return default(object);
        }
    }