using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue164
{
    public class TestOption164Parser
    {
        [Production("root: INT [PLUS|MINUS|TIMES|DIVIDE] INT")]
        public int Root(Token<TestOption164Lexer> left, Token<TestOption164Lexer> op, Token<TestOption164Lexer> right)
        {
            int l = left.IntValue;
            int r = right.IntValue;
            switch (op.TokenID)
            {
                case TestOption164Lexer.PLUS:
                    return l + r;
                case TestOption164Lexer.MINUS:
                    return l - r;
                case TestOption164Lexer.TIMES:
                    return l * r;
                case TestOption164Lexer.DIVIDE:
                    return l / r;
                default:
                    return 0;
            }
        }
    }
}