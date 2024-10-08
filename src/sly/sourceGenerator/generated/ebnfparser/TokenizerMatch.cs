using sly.lexer;

namespace sly.sourceGenerator.generated.ebnfparser;

public class TokenizerMatch
{

    public EbnfRuleToken TokenId => Token.TokenID;
    
    public Token<EbnfRuleToken> Token { get; set; }
    
    public bool Matches { get; set; }
    
    public bool IsWhiteSpace { get; set; }
    public int NewPosition { get; set; }
    
    public TokenizerMatch()
    {
        Matches = false;
    }

    public TokenizerMatch(Token<EbnfRuleToken> token, int newPosition)
    {
        Token = token;
        Matches = true;
        NewPosition = newPosition;
    }
    
    public static TokenizerMatch NoMatch() => new TokenizerMatch() { Matches = false };
    
    public static TokenizerMatch WhiteSpace(int newPosition) => new TokenizerMatch() { Matches = true, IsWhiteSpace = true, NewPosition = newPosition};
    
    public static TokenizerMatch Found(Token<EbnfRuleToken> token, int newPosition) => new TokenizerMatch() { Matches = true, Token = token, IsWhiteSpace = false, NewPosition = newPosition };
    
}