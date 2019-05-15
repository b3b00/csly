using System;
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
            token.SpanValue = new ReadOnlyMemory<char>(token.Value.ToUpper().ToCharArray());
            
            return token;
        } 
        
    }
}