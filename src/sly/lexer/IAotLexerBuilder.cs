using System.Collections.Generic;
using aot.parser;

namespace sly.lexer;

public interface IAotLexerBuilder<IN> where IN : struct
{
    
    public IAotLexerBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main);

    public IAotLexerBuilder<IN> Integer(IN tokenId, int channel = Channels.Main);

    public IAotLexerBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main);
    
    public IAotLexerBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main);

    public IAotLexerBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main);
    public IAotLexerBuilder<IN> AlphaNumId(IN tokenId);
    
    public IAotLexerBuilder<IN> AlphaId(IN tokenId);
    
    public IAotLexerBuilder<IN> AlphaNumDashId(IN tokenId);
    
    public IAotLexerBuilder<IN> CustomId(IN tokenId, string start, string end);

    public IAotLexerBuilder<IN> Labeled(string lang, string label);

    public IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start);

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end);

    public IAotLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null);
    
    ILexer<IN> Build();
}