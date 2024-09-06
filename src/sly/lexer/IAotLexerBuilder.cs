using System;
using System.Collections.Generic;
using aot.parser;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer;

public interface IAotLexerBuilder<IN> where IN : struct
{
    
    public IAotLexerBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main, params string[] modes);

    public IAotLexerBuilder<IN> Integer(IN tokenId, int channel = Channels.Main, params string[] modes);

    public IAotLexerBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main, params string[] modes);
    
    public IAotLexerBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main, params string[] modes);
    
    public IAotLexerBuilder<IN> Keyword(IN tokenId, string[] tokens, int channel = Channels.Main, params string[] modes);

    public IAotLexerBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main, params string[] modes);
    
    public IAotLexerBuilder<IN> Character(IN tokenId, string delimiter ="'", string escapeChar = "\\", int channel = Channels.Main, params string[] modes);
    public IAotLexerBuilder<IN> AlphaNumId(IN tokenId, params string[] modes);
    
    public IAotLexerBuilder<IN> AlphaId(IN tokenId, params string[] modes);
    
    public IAotLexerBuilder<IN> AlphaNumDashId(IN tokenId, params string[] modes);
    
    public IAotLexerBuilder<IN> CustomId(IN tokenId, string start, string end, params string[] modes);

    IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes);

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes);

    public IAotLexerBuilder<IN> UpTo(IN tokenId, string pattern, int channl = Channels.Main, params string[] modes);
    
    public IAotLexerBuilder<IN> UpTo(IN tokenId, string[] patterns, int channl = Channels.Main, params string[] modes);
    
    public IAotLexerBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEOL = false);

    public IAotLexerBuilder<IN> Extension(IN tokenId, int channel = Channels.Main, params string[] modes);

    public IAotLexerBuilder<IN> UseExtensionBuilder(Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder);

    public IAotLexerBuilder<IN> UseLexerPostProcessor(LexerPostProcess<IN> lexerPostProcessor);

    public IAotLexerBuilder<IN> UseTokenCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback);
    
    public IAotLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null);

    public IAotLexerBuilder<IN> IgnoreEol(bool ignore);

    public IAotLexerBuilder<IN> IgnoreWhiteSpace(bool ignore);

    public IAotLexerBuilder<IN> UseWhiteSpaces(char[] whiteSpaces);

    public IAotLexerBuilder<IN> IgnoreKeywordCase(bool ignore = true);

    public IAotLexerBuilder<IN> IsIndentationAware(bool isAware = true);

    public IAotLexerBuilder<IN> UseIndentations(string indentation);

    public IAotLexerBuilder<IN> Push(IN tokenId, string targetMode);
    
    public IAotLexerBuilder<IN> Pop(IN tokenId);
    
    public IAotLexerBuilder<IN> Label(IN tokenId, string lang, string label);

    public IAotLexerBuilder<IN> WithCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback);
    
    public BuildResult<ILexer<IN>> Build(string lang);
    
    
    
    
    
    
}