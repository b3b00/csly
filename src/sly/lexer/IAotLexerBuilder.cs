using System.Collections.Generic;

namespace sly.lexer;

public interface IAotLexerBuilder<T> where T : struct
{
    
    public IAotLexerBuilder<T> Double(T tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main);

    public IAotLexerBuilder<T> Integer(T tokenId, int channel = Channels.Main);

    public IAotLexerBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main);
    
    public IAotLexerBuilder<T> Keyword(T tokenId, string token, int channel = Channels.Main);

    public IAotLexerBuilder<T> AlphaNumId(T tokenId);

    public IAotLexerBuilder<T> Labeled(string lang, string label);

    public IAotLexerBuilder<T> SingleLineComment(T tokenId, string start);

    public IAotLexerBuilder<T> MultiLineComment(T tokenId, string start, string end);

    public IAotLexerBuilder<T> WithExplicitTokens(IList<string> explicitTokens = null);
    
    ILexer<T> Build();
}