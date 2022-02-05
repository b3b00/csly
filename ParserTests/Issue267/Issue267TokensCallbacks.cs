using System;
using GenericLexerWithCallbacks;
using sly.lexer;

namespace ParserTests.Issue267
{
    public class Issue267TokensCallbacks
    {
        [TokenCallback((int)Issue267Token.Identifier)]
        public static Token<Issue267Token> TranslateIdentifier(Token<Issue267Token> token)
        {
            string value = token.Value;
            if (value.Equals("declare", StringComparison.InvariantCultureIgnoreCase))
            {
                token.TokenID = Issue267Token.Declare;
            }
            // here manage other keywords
		
            return token;
        } 
    }
}