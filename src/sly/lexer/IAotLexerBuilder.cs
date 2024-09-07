using System;
using System.Collections.Generic;
using aot.parser;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer;

public interface IAotLexemeBuilder<IN> : IAotLexerBuilder<IN> where IN : struct {
    
    IAotLexemeBuilder<IN> WithLabel(string lang, string label);
    
    IAotLexemeBuilder<IN> WithLabels(params (string lang, string label)[] labels);
    
    IAotLexemeBuilder<IN> WithModes(params string[] modes);
    
    IAotLexemeBuilder<IN> OnChannel(int channel);
    
    IAotLexemeBuilder<IN> PopMode();
    
    IAotLexemeBuilder<IN> PushToMode(string targetMode);
    
    }

public interface IAotLexerBuilder<IN> where IN : struct
{
    
    public IAotLexemeBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main, params string[] modes);

    public IAotLexemeBuilder<IN> Integer(IN tokenId, int channel = Channels.Main, params string[] modes);

    public IAotLexemeBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main, params string[] modes);
    
    public IAotLexemeBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main, params string[] modes);
    
    public IAotLexemeBuilder<IN> Keyword(IN tokenId, string[] tokens, int channel = Channels.Main, params string[] modes);

    public IAotLexemeBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main, params string[] modes);
    
    public IAotLexemeBuilder<IN> Character(IN tokenId, string delimiter ="'", string escapeChar = "\\", int channel = Channels.Main, params string[] modes);
    public IAotLexemeBuilder<IN> AlphaNumId(IN tokenId, params string[] modes);
    
    public IAotLexemeBuilder<IN> AlphaId(IN tokenId, params string[] modes);
    
    public IAotLexemeBuilder<IN> AlphaNumDashId(IN tokenId, params string[] modes);
    
    public IAotLexemeBuilder<IN> CustomId(IN tokenId, string start, string end, params string[] modes);

    IAotLexemeBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes);

    public IAotLexemeBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes);

    public IAotLexemeBuilder<IN> UpTo(IN tokenId, string pattern, int channl = Channels.Main, params string[] modes);
    
    public IAotLexemeBuilder<IN> UpTo(IN tokenId, string[] patterns, int channl = Channels.Main, params string[] modes);
    
    public IAotLexemeBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEOL = false);

    public IAotLexemeBuilder<IN> Extension(IN tokenId, int channel = Channels.Main, params string[] modes);

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