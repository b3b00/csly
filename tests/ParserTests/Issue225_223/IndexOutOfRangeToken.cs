using sly.lexer;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    [Lexer(IgnoreWS = true, IgnoreEOL = true, KeyWordIgnoreCase = true)]
    public enum IndexOutOfRangeToken
    {
        [Lexeme(GenericToken.KeyWord, "or")]
        OR,
        [Lexeme(GenericToken.KeyWord, "and")]
        AND,
        [Lexeme(GenericToken.KeyWord, "not")]
        NOT,
        [Lexeme(GenericToken.SugarToken, "+")]
        PLUS,
        [Lexeme(GenericToken.SugarToken, "-")]
        DASH,
        [Lexeme(GenericToken.KeyWord, "to")]
        TO,
        [Lexeme(GenericToken.SugarToken, "/")]
        SLASH,
        [Lexeme(GenericToken.KeyWord, "now")]
        NOW,
        [Lexeme(GenericToken.KeyWord, "t")]
        T,
        [Lexeme(GenericToken.KeyWord, "z")]
        Z,
        [Lexeme(GenericToken.KeyWord, "seconds")]
        SECONDS,
        [Lexeme(GenericToken.KeyWord, "second")]
        SECOND,
        [Lexeme(GenericToken.KeyWord, "minutes")]
        MINUTES,
        [Lexeme(GenericToken.KeyWord, "minute")]
        MINUTE,
        [Lexeme(GenericToken.KeyWord, "hours")]
        HOURS,
        [Lexeme(GenericToken.KeyWord, "hour")]
        HOUR,
        [Lexeme(GenericToken.KeyWord, "days")]
        DAYS,
        [Lexeme(GenericToken.KeyWord, "day")]
        DAY,
        [Lexeme(GenericToken.KeyWord, "weeks")]
        WEEKS,
        [Lexeme(GenericToken.KeyWord, "week")]
        WEEK,
        [Lexeme(GenericToken.KeyWord, "months")]
        MONTHS,
        [Lexeme(GenericToken.KeyWord, "month")]
        MONTH,
        [Lexeme(GenericToken.KeyWord, "years")]
        YEARS,
        [Lexeme(GenericToken.KeyWord, "year")]
        YEAR,
        
        [Lexeme(GenericToken.String, "'", "\\")]
        [Lexeme(GenericToken.String, "\"", "\\")]
        STRING,
        
        [Lexeme(GenericToken.SugarToken, ":")]
        COLON,

        [Lexeme(GenericToken.SugarToken, "^")]
        CARET,

        [Lexeme(GenericToken.SugarToken, "[")]
        LBRACKET,
        [Lexeme(GenericToken.SugarToken, "]")]
        RBRACKET,
        
        [Lexeme(GenericToken.SugarToken, "(")]
        LPAREN,
        [Lexeme(GenericToken.SugarToken, ")")]
        RPAREN,

        [Lexeme(GenericToken.Double)]
        DOUBLE,

        [Lexeme(GenericToken.Int)]
        INT,
        
        [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "_A-Za-z0-9.@#&\\=*~", "_A-Za-z0-9.@#+&/\\=*~+")]
        VALUE
    }
}