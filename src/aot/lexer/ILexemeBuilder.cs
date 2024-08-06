using sly.lexer;

namespace aot.lexer;

public interface ILexemeBuilder<T> where T : struct
{
    
    public ILexemeBuilder<T> Double(T tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main);

    public ILexemeBuilder<T> Integer(T tokenId, int channel = Channels.Main);

    public ILexemeBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main);
    
    public ILexemeBuilder<T> Keyword(T tokenId, string token, int channel = Channels.Main);

    public ILexemeBuilder<T> AlphaNumId(T tokenId);
    
    ILexer<T> Build();
}