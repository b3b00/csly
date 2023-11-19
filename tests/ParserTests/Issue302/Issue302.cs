using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue302
{
 public enum Issue302Token
    {
        // Base types
        [Lexeme(@"[0-9]+")]
        INT = 1,

        [Lexeme(@"TRUE|FALSE")]
        BOOL,

        [Lexeme(@"'[^']*'")]
        STRING,

        // Variables and such
        [Lexeme(@"[\w]+")]
        ID,

        [Lexeme(@"\$\$[\w]+")]
        PARAMETER,

        [Lexeme(@"\+")]
        PLUS,

        [Lexeme("[ \t\n]+", isSkippable: true)]
        WS
    }
    public class Issue302Parser
    {
        [Operation((int) Issue302Token.PLUS, Affix.InFix, Associativity.Left, 10)]
        public int Comparison(int e1, Token<Issue302Token> op, int e2)
        {
            return e1+e2;
        }

        [Operand]
        [Production("expr: term")]
        public int Operand(int f)
        {
            return f;
        }

        [Production("term: INT")]
        public int PrimaryInt(Token<Issue302Token> tok)
        {
            return tok.IntValue;
        }

        [Production("term: STRING")]
        public int PrimaryStr(Token<Issue302Token> tok)
        {
            return -1;
        }

        [Production("term: BOOL")]
        public int PrimaryBool(Token<Issue302Token> tok)
        {
            return tok.Value == "TRUE"  ? 1 : 0;
        }

        [Production("term: ID")]
        public int PrimaryId(Token<Issue302Token> tok)
        {
            return 0;
        }

        [Production("term: PARAMETER")]
        public int PrimaryParam(Token<Issue302Token> tok)
        {
            return 0;
        }
    }
}