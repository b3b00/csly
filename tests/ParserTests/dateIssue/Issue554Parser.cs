using sly.lexer;
using sly.parser.generator;

namespace ParserTests.dateIssue;


    [ParserRoot("root")]
[FirstDerivation]
    public class Issue554Parser
    {
        [Production("root : DOUBLE INT DATE")]
        public string root_DOUBLE_INT_DATE(Token<Issue554lexer> d, Token<Issue554lexer> i, Token<Issue554lexer> date)
        {
            return $"{d.Value}/{i.Value}/{date.Value}";  
        }
    }