using sly.lexer;

namespace ParserTests.Issue218;

public enum Token218
{
    [Lexeme(GenericToken.Double)]
    DOUBLE,
    [Lexeme(GenericToken.Int)]
    INT,
    [Lexeme(GenericToken.String, "\"", "\\")]
    STRING,
    [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
    IDENTIFIER,
    [Lexeme(GenericToken.SugarToken, ";")]
    SEMICOLON,
    [Lexeme(GenericToken.SugarToken, ",")]
    COMMA,
    [Lexeme(GenericToken.SugarToken, "=")]
    ASSIGNMENT,
    [Lexeme(GenericToken.SugarToken, "(")]
    LPAREN,
    [Lexeme(GenericToken.SugarToken, ")")]
    RPAREN,
    [Lexeme(GenericToken.SugarToken, "{")]
    LBRACE,
    [Lexeme(GenericToken.SugarToken, "}")]
    RBRACE,
    [Lexeme(GenericToken.SugarToken, "[")]
    LBRACKET,
    [Lexeme(GenericToken.SugarToken, "]")]
    RBRACKET,
    [Lexeme(GenericToken.SugarToken, ">")]
    GREATER,
    [Lexeme(GenericToken.SugarToken, "<")]
    LESSER,
    [Lexeme(GenericToken.SugarToken, "==")]
    DOUBLEEQUALS,
    [Lexeme(GenericToken.SugarToken, "!=")]
    DIFFERENT,
    [Lexeme(GenericToken.SugarToken, "+")]
    PLUS,
    [Lexeme(GenericToken.SugarToken, "-")]
    MINUS,
    [Lexeme(GenericToken.SugarToken, "*")]
    TIMES,
    [Lexeme(GenericToken.SugarToken, "/")]
    DIVIDE,
    [Lexeme(GenericToken.SugarToken, "%")]
    MODULUS,
    [Lexeme(GenericToken.SugarToken, "++")]
    PLUSPLUS,
    [Lexeme(GenericToken.SugarToken, "--")]
    MINUSMINUS,
    [Lexeme(GenericToken.KeyWord, "if")]
    IF,
    [Lexeme(GenericToken.KeyWord, "else")]
    ELSE,
    [Lexeme(GenericToken.KeyWord, "loop")]
    LOOP,
    [Lexeme(GenericToken.KeyWord, "while")]
    WHILE,
    [Lexeme(GenericToken.KeyWord, "true")]
    TRUE,
    [Lexeme(GenericToken.KeyWord, "false")]
    FALSE,
    [Lexeme(GenericToken.KeyWord, "break")]
    BREAK,
    [Lexeme(GenericToken.KeyWord, "continue")]
    CONTINUE,
    [Lexeme(GenericToken.KeyWord, "return")]
    RETURN,
    [Lexeme(GenericToken.KeyWord, "function")]
    FUNCTION,
    [Lexeme(GenericToken.KeyWord, "import")]
    IMPORT,
    [Lexeme(GenericToken.KeyWord, "new")]
    NEW,
    [Comment("//", "/*", "*/")]
    COMMENT
}