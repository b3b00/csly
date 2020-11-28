using sly.lexer;

namespace ParserExample
{
    public enum ChannelLexer
    {
        
        [Lexeme(GenericToken.Identifier)] ID = 20,
        
        [Lexeme(GenericToken.KeyWord,"toto")] TOTO = 23,

        [Lexeme(GenericToken.String)] STRING = 21,

        [Lexeme(GenericToken.Int,channel:101)] INT = 22,
        
        [SingleLineComment("//", channel:2)]
        COMMENT = 458
        
        
    }
}