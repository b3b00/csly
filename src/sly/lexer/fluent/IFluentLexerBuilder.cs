using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer.fluent;



public interface IFluentLexerBuilder<IN> where IN : struct
{
    
    public IFluentLexemeBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main);

    public IFluentLexemeBuilder<IN> Int(IN tokenId, int channel = Channels.Main);
    
    public IFluentLexemeBuilder<IN> Integer(IN tokenId, int channel = Channels.Main);

    public IFluentLexemeBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main);

    public IFluentLexemeBuilder<IN> Date(IN tokenId, DateFormat format, char separator);

    public IFluentLexemeBuilder<IN> Hexa(IN tokenId, string prefix);
    
    public IFluentLexemeBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main);
    
    public IFluentLexemeBuilder<IN> Keyword(IN tokenId, string[] tokens, int channel = Channels.Main);

    public IFluentLexemeBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main);
    
    public IFluentLexemeBuilder<IN> Character(IN tokenId, string delimiter ="'", string escapeChar = "\\", int channel = Channels.Main);
    public IFluentLexemeBuilder<IN> AlphaNumId(IN tokenId);
    
    public IFluentLexemeBuilder<IN> AlphaId(IN tokenId);
    
    public IFluentLexemeBuilder<IN> AlphaNumDashId(IN tokenId);
    
    public IFluentLexemeBuilder<IN> CustomId(IN tokenId, string start, string end);

    IFluentLexemeBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments);

    public IFluentLexemeBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments);

    public IFluentLexemeBuilder<IN> UpTo(IN tokenId, string pattern);
    
    public IFluentLexemeBuilder<IN> UpTo(IN tokenId, params string[] patterns);
    
    public IFluentLexemeBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEol = false);

    public IFluentLexemeBuilder<IN> Extension(IN tokenId, int channel = Channels.Main);

    public IFluentLexerBuilder<IN> UseExtensionBuilder(Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder);

    public IFluentLexerBuilder<IN> UseLexerPostProcessor(LexerPostProcess<IN> lexerPostProcessor);
    
    public IFluentLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null);

    public IFluentLexerBuilder<IN> IgnoreEol(bool ignore);

    public IFluentLexerBuilder<IN> IgnoreWhiteSpace(bool ignore);

    public IFluentLexerBuilder<IN> UseWhiteSpaces(char[] whiteSpaces);

    public IFluentLexerBuilder<IN> IgnoreKeywordCase(bool ignore = true);

    public IFluentLexerBuilder<IN> IsIndentationAware(bool isAware = true);

    public IFluentLexerBuilder<IN> UseIndentations(string indentation);

    public IFluentLexerBuilder<IN> Push(IN tokenId, string targetMode);
    
    public IFluentLexerBuilder<IN> Pop(IN tokenId);


    public IFluentLexerBuilder<IN> WithCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback);
    
    public BuildResult<ILexer<IN>> Build(string lang);
    
    
    
    
    
    
}