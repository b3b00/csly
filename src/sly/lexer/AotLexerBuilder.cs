using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.i18n;
using sly.lexer.fsm;

namespace sly.lexer;

public class AotLexerBuilder<IN> :  IAotLexerBuilder<IN> where IN : struct
{
    private bool _ignoreWS = true;

    private bool _ignoreEOL = true;

    private char[]_whiteSpaces;

    private bool _keyWordIgnoreCase = false;

    private bool _indentationAware = false;

    private string _indentation;

    private List<(IN, Func<Token<IN>, Token<IN>>)> _tokenCallbacks = new List<(IN, Func<Token<IN>, Token<IN>>)>();
    
    private Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)> _lexemes;

    private Dictionary<IN, List<CommentAttribute>> _comments;

    private IN? _currentLexeme;
    private IList<string> _explicitTokens;

    private Action<IN, LexemeAttribute, GenericLexer<IN>> _extensionBuilder;
    private LexerPostProcess<IN> _lexerPostProcessor;
    private List<(IN tokenId,Func<Token<IN>, Token<IN>> callback)> _callbacks;

    private Dictionary<IN , string > _modePushers;
    
    private List<IN> _modePopers;

    private Dictionary<IN, List<string>> _modes;


    public static IAotLexerBuilder<IN> NewBuilder() 
    {
        return new AotLexerBuilder<IN>();
    }

    
    
    private AotLexerBuilder()
    {
        _lexemes = new Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
        _comments = new Dictionary<IN, List<CommentAttribute>>();
        _callbacks = new List<(IN tokenId, Func<Token<IN>, Token<IN>> callback)>();
        _modes = new Dictionary<IN, List<string>>();
        _modePushers = new Dictionary<IN, string>();
        _modePopers = new List<IN>();
    }

    private void AddComment(IN tokenId, CommentAttribute comment, params string[] modes)
    {
        _currentLexeme = tokenId;
        List<CommentAttribute> comments = null;
        if (!_comments.TryGetValue(tokenId, out comments))
        {
            comments = new List<CommentAttribute>();
        }
        comments.Add(comment);
        _comments[tokenId] = comments;
        _lexemes[tokenId] = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
    }
    
    private void Add(IN tokenId, LexemeAttribute lexeme, LexemeLabelAttribute label, params string[] modes)
    {
        _currentLexeme = tokenId;
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes = (null,null);
        if (!_lexemes.TryGetValue(tokenId, out lexemes))
        {
            lexemes = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
        }
        lexemes.tokens.Add(lexeme);
        lexemes.labels.Add(label);
        _lexemes[tokenId] = lexemes;
        _modes[tokenId] = modes != null && modes.Length > 0 ? modes.ToList() : new List<string>();
    }

    private void AddLabel(IN tokenId, string lang, string label)
    {
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes = (null,null);
        if (_lexemes.TryGetValue(tokenId, out lexemes))
        {
            if (lexemes.labels == null)
            {
                lexemes.labels = new List<LexemeLabelAttribute>();
            }
            lexemes.labels.Add(new LexemeLabelAttribute(lang, label));
        }
        else
        {
            throw new InvalidOperationException($"{_currentLexeme.Value} does not exist !");
        }
    }

    
    public IAotLexerBuilder<IN> IgnoreEol(bool ignore)
    {
        _ignoreEOL = ignore;
        return this;
    }
    
    public IAotLexerBuilder<IN> IgnoreWhiteSpace(bool ignore)
    {
        _ignoreWS = ignore;
        return this;
    }

    public IAotLexerBuilder<IN> UseWhiteSpaces(char[] whiteSpaces)
    {
        _whiteSpaces = whiteSpaces;
        return this;
    }

    public IAotLexerBuilder<IN> IgnoreKeywordCase(bool ignore = true)
    {
        _keyWordIgnoreCase = ignore;
        return this;
    }

    public IAotLexerBuilder<IN> IsIndentationAware(bool isAware = true)
    {
        _indentationAware = isAware;
        return this;
    }

    public IAotLexerBuilder<IN> UseIndentations(string indentation)
    {
        _indentation = indentation;
        return this;
    }

    public IAotLexerBuilder<IN> Push(IN tokenId, string targetMode)
    {
        _modePushers[tokenId] = targetMode;
        return this;
    }

    public IAotLexerBuilder<IN> Pop(IN tokenId)
    {
        _modePopers.Add(tokenId);
        return this;
    }

    public IAotLexerBuilder<IN> SingleLineComment(string start, params string[] modes)
    {
        // TODO AOT ??? 
        return this;
    }
    
    public IAotLexerBuilder<IN> MultiLineComment(string start, params string[] modes)
    {
        // TODO AOT ???
        return this;
    }

    public IAotLexerBuilder<IN> Extension(IN tokenId, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.Extension,channel:channel), null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".", int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId, new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) , null, modes);
        return this;
    }

    public IAotLexerBuilder<IN> Integer(IN tokenId, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Int, channel:Channels.Main) , null, modes);
        return this;
    }

    public IAotLexerBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token), null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, parameters:token) ,null, modes);
        return this;
    }

    public IAotLexerBuilder<IN> Keyword(IN tokenId, string[] tokens, int channel = Channels.Main,
        params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, tokens.ToArray()) ,null, modes);
        return this;
    }

    public IAotLexerBuilder<IN> AlphaNumId(IN tokenId, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) , null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> AlphaId(IN tokenId, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Alpha, channel:Channels.Main) , null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> AlphaNumDashId(IN tokenId, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumericDash, channel:Channels.Main) , null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> CustomId(IN tokenId, string start, string end, params string[] modes)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Custom, start, end, channel:Channels.Main) , null, modes);
        return this;
    }

    public IAotLexerBuilder<IN> Label(IN tokenId, string lang, string label)
    {
        AddLabel(tokenId, lang, label);
        return this;
    }

    public IAotLexerBuilder<IN> WithCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback)
    {
        _tokenCallbacks.Add((tokenId, callback));
        return this;
    }

    public IAotLexerBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.String,channel, delimiter,escapeChar),null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> Character(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.Char,channel, delimiter,escapeChar),null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes)
    {
        AddComment(tokenId, new SingleLineCommentAttribute(start, doNotIgnore, channel));
        return this;
    }

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments, params string[] modes)
    {
        AddComment(tokenId, new MultiLineCommentAttribute(start, end, doNotIgnore, channel));
        return this;
    }

    public IAotLexerBuilder<IN> UpTo(IN tokenId, string pattern, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.UpTo,channel, pattern),null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> UpTo(IN tokenId, string[] patterns, int channel = Channels.Main, params string[] modes)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.UpTo,channel, patterns),null, modes);
        return this;
    }
    
    public IAotLexerBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEOL = false)
    {
        Add(tokenId, new LexemeAttribute(regex,isSkippable,isEOL),null);
        return this;
    }

    public IAotLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null)
    {
        _explicitTokens = explicitTokens;
        return this;
    }

    public IAotLexerBuilder<IN> UseExtensionBuilder(Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder)
    {
        _extensionBuilder = extensionBuilder;
        return this;
    }

    public IAotLexerBuilder<IN> UseLexerPostProcessor(LexerPostProcess<IN> lexerPostProcessor)
    {
        _lexerPostProcessor = lexerPostProcessor;
        return this;
    }

    public IAotLexerBuilder<IN> UseTokenCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback)
    {
        _callbacks.Add((tokenId,callback));
        return this;
    }
    
    public BuildResult<ILexer<IN>> Build(string lang)
    {
        var lexerConfig = new LexerAttribute()
        {
            Indentation = _indentation,
            IndentationAWare = _indentationAware,
            WhiteSpace = _whiteSpaces,
            IgnoreWS = _ignoreWS,
            IgnoreEOL = _ignoreEOL,
            KeyWordIgnoreCase = _keyWordIgnoreCase
        };
        
        BuildResult<ILexer<IN>> r = new BuildResult<ILexer<IN>>();

        Func<KeyValuePair<IN, (List<LexemeAttribute> lexemes, List<LexemeLabelAttribute> labels)>, (List<string> modes,
            bool isModePopper, string pushTarget)> modesGetter =
            lexeme =>
            {
                List<string> modes = null;
                _modes.TryGetValue(lexeme.Key, out modes);
                bool isModePopper = _modePopers.Exists(x => x.Equals(lexeme.Key));
                string pushMode = null;
                if (!_modePushers.TryGetValue(lexeme.Key, out pushMode))
                {
                    pushMode = null;
                }

                return (modes, isModePopper, pushMode);
            }; 
        
        
        
        var lexerResult = LexerBuilder.BuildLexer(r, _extensionBuilder, lang, _lexerPostProcessor, _explicitTokens, _lexemes, lexerConfig, _comments, modesGetter, () => _callbacks);
        if (lexerResult.IsOk && lexerResult.Result is GenericLexer<IN> genericLexer)
        {
            if (_callbacks.Any())
            {
                foreach (var callback in _callbacks)
                {
                    genericLexer.AddCallBack(callback.tokenId, callback.callback);
                }
            }
        }
        return lexerResult;
    }
}