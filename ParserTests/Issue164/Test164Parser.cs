using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue164
{
    public class Test164Parser
    {
        [Production("root: INT [PLUS|MINUS|TIMES|DIVIDE] INT")]
        public int root(Token<Test164Lexer> left, Token<Test164Lexer> op, Token<Test164Lexer> right)
        {
            int res = 0;
            int l = left.IntValue;
            int r = right.IntValue;
            switch (op.TokenID)
            {
                case Test164Lexer.PLUS:
                    return l + r;
                case Test164Lexer.MINUS:
                    return l - r;
                case Test164Lexer.TIMES:
                    return l * r;
                case Test164Lexer.DIVIDE:
                    return l / r;
                default:
                    return 0;
            }
        }
    }
}