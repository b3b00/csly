using System;
using System.Collections.Generic;
using aot.parser;
using sly.buildresult;
using sly.lexer.fsm;

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

    IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments);

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments);

    public IAotLexerBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEOL = false);

    public IAotLexerBuilder<IN> Extension(IN tokenId, int channel = Channels.Main);

    public IAotLexerBuilder<IN> UseExtensionBuilder(Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder);

    public IAotLexerBuilder<IN> UseLexerPostProcessor(LexerPostProcess<IN> lexerPostProcessor);
    
    public IAotLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null);

    public IAotLexerBuilder<IN> IgnoreEol(bool ignore);

    public IAotLexerBuilder<IN> IgnoreWhiteSpace(bool ignore);

    public IAotLexerBuilder<IN> UseWhiteSpaces(char[] whiteSpaces);

    public IAotLexerBuilder<IN> IgnoreKeywordCase(bool ignore = true);

    public IAotLexerBuilder<IN> IsIndentationAware(bool isAware = true);

    public IAotLexerBuilder<IN> UseIndentations(string indentation);
    
    public BuildResult<ILexer<IN>> Build();
}