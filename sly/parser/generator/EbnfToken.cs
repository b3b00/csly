using sly.lexer;

namespace sly.parser.generator
{
    public enum EbnfToken
    {
        [Lexeme("^[A-Za-z][A-Za-z0-9_]*")] IDENTIFIER = 1,
        [Lexeme(":")] COLON = 2,
        [Lexeme("\\*")] ZEROORMORE = 3,
        [Lexeme("\\+")] ONEORMORE = 4,
        [Lexeme("[ \\t]+", true)] WS = 5,
        [Lexeme("^\\?")] OPTION = 6,
        [Lexeme("^\\[d\\]")] DISCARD = 7,

        [Lexeme("^\\(")] LPAREN = 8,

        [Lexeme("^\\)")] RPAREN = 9,

        [Lexeme("[\\n\\r]+", true, true)] EOL = 10
    }
}