using sly.lexer;

namespace GenericLexerWithCallbacks
{
    public class TestCallbacks
    {

        [TokenCallback((int)Tokens.IDENTIFIER)]
        public Token<Tokens> TranslateID(Token<Tokens> token)
        {
            return token;
        } 
        
    }
}