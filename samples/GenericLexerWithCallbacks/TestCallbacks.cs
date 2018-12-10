using sly.lexer;

namespace GenericLexerWithCallbacks
{
    public class TestCallbacks
    {

        [TokenCallback((int)Tokens.IDENTIFIER)]
        public static Token<Tokens> TranslateIdentifier(Token<Tokens> token)
        {
            token.Value = token.Value.ToUpper();
            return token;
        } 
        
    }
}