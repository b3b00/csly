using sly.lexer;

namespace GenericLexerWithCallbacks
{
    public class TestCallbacks
    {

        [TokenCallback((int)CallbackTokens.IDENTIFIER)]
        public static Token<CallbackTokens> TranslateIdentifier(Token<CallbackTokens> token)
        {
            if (token.Value.StartsWith("b"))
            {
                token.TokenID = CallbackTokens.SKIP;
            }
            token.Value = token.Value.ToUpper();
            
            return token;
        } 
        
    }
}