using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.i18n;
using sly.lexer.fsm;

namespace sly.lexer;

public class AotLexerBuilder<IN> :  IAotLexerBuilder<IN>, IAotLexemeBuilder<IN> where IN : struct
{
    private bool _ignoreWS = true;

    private bool _ignoreEOL = true;

    private char[]_whiteSpaces;

    private bool _keyWordIgnoreCase = false;

    private bool _indentationAware = false;

    private string _indentation;

    
    
    private Dictionary<IN, (List<LexemeAttribute> definitions, List<LexemeLabelAttribute> labels) > _lexemesDefinitionsAndLabels;

    private Dictionary<IN, List<CommentAttribute>> _comments;

    private IN? _currentLexeme;
    private IList<string> _explicitTokens;

    private Action<IN, LexemeAttribute, GenericLexer<IN>> _extensionBuilder;
    private LexerPostProcess<IN> _lexerPostProcessor;

    private List<(IN tokenId, Func<Token<IN>, Token<IN>> callback)> _callbacks = new();

    private Dictionary<IN , string > _modePushers;
    
    private List<IN> _modePopers;

    private Dictionary<IN, List<string>> _modes;


    public static IAotLexerBuilder<IN> NewBuilder() 
    {
        return new AotLexerBuilder<IN>();
    }

    
    
    private AotLexerBuilder()
    {
        _lexemesDefinitionsAndLabels = new Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
        _comments = new Dictionary<IN, List<CommentAttribute>>();
        _callbacks = new List<(IN tokenId, Func<Token<IN>, Token<IN>> callback)>();
        _modes = new Dictionary<IN, List<string>>();
        _modePushers = new Dictionary<IN, string>();
        _modePopers = new List<IN>();
    }

    private void AddComment(IN tokenId, CommentAttribute comment)
    {
        _currentLexeme = tokenId;
        List<CommentAttribute> comments = null;
        if (!_comments.TryGetValue(tokenId, out comments))
        {
            comments = new List<CommentAttribute>();
        }
        comments.Add(comment);
        _comments[tokenId] = comments;
        _lexemesDefinitionsAndLabels[tokenId] = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
    }
    
    private void Add(IN tokenId, LexemeAttribute lexeme, LexemeLabelAttribute label)
    {
        _currentLexeme = tokenId;
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes;
        if (!_lexemesDefinitionsAndLabels.TryGetValue(tokenId, out lexemes))
        {
            lexemes = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
        }

        if (lexeme != null)
        {
            lexemes.tokens.Add(lexeme);
        }

        if (label != null)
        {
            lexemes.labels.Add(label);
        }

        _lexemesDefinitionsAndLabels[tokenId] = lexemes;
    }

    private void AddLabel(IN tokenId, string lang, string label)
    {
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes = (null,null);
        if (_lexemesDefinitionsAndLabels.TryGetValue(tokenId, out lexemes))
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

 

    public IAotLexemeBuilder<IN> SingleLineComment(string start)
    {
        // TODO AOT ??? 
        return this;
    }
    
    public IAotLexemeBuilder<IN> MultiLineComment(string start)
    {
        // TODO AOT ???
        return this;
    }

    public IAotLexemeBuilder<IN> Extension(IN tokenId, int channel = Channels.Main)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.Extension,channel:channel), null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".", int channel = Channels.Main)
    {
        Add(tokenId, new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) , null);
        return this;
    }

    public IAotLexemeBuilder<IN> Integer(IN tokenId, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Int, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexemeBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token), null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, parameters:token) ,null);
        return this;
    }

    public IAotLexemeBuilder<IN> Keyword(IN tokenId, string[] tokens, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, tokens.ToArray()) ,null);
        return this;
    }

    public IAotLexemeBuilder<IN> AlphaNumId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> AlphaId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Alpha, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> AlphaNumDashId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumericDash, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> CustomId(IN tokenId, string start, string end)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Custom, start, end, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexerBuilder<IN> WithCallback(IN tokenId, Func<Token<IN>, Token<IN>> callback)
    {
        _callbacks.Add((tokenId, callback));
        return this;
    }

    public IAotLexemeBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.String,channel, delimiter,escapeChar),null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> Character(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.Char,channel, delimiter,escapeChar),null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments)
    {
        AddComment(tokenId, new SingleLineCommentAttribute(start, doNotIgnore, channel));
        return this;
    }

    public IAotLexemeBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments)
    {
        AddComment(tokenId, new MultiLineCommentAttribute(start, end, doNotIgnore, channel));
        return this;
    }

    public IAotLexemeBuilder<IN> UpTo(IN tokenId, string pattern)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.UpTo, pattern),null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> UpTo(IN tokenId, params string[] patterns)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.UpTo, patterns),null);
        return this;
    }
    
    public IAotLexemeBuilder<IN> Regex(IN tokenId, string regex, bool isSkippable = false, bool isEOL = false)
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
        
        
        
        var lexerResult = LexerBuilder.BuildLexer(r, _extensionBuilder, lang, _lexerPostProcessor, _explicitTokens, _lexemesDefinitionsAndLabels, lexerConfig, _comments, modesGetter, () => _callbacks);
       
        return lexerResult;
    }
    
    
    #region lexeme builder
    
    public IAotLexemeBuilder<IN> WithLabel(string lang, string label)
    {
        (List<LexemeAttribute> lexeme, List<LexemeLabelAttribute> labels) currentLexeme;
        if (!_lexemesDefinitionsAndLabels.TryGetValue(_currentLexeme.Value, out currentLexeme))
        {
            // impossible !
            currentLexeme = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
        }

        if (!currentLexeme.labels.Any(x => x.Language == lang))
        {
            currentLexeme.labels.Add(new LexemeLabelAttribute(lang, label));
        }

        _lexemesDefinitionsAndLabels[_currentLexeme.Value] = currentLexeme;
        return this;
    }

    public IAotLexemeBuilder<IN> WithLabels(params (string lang, string label)[] labels)
    {
        (List<LexemeAttribute> lexeme, List<LexemeLabelAttribute> labels) currentLexeme;
        if (!_lexemesDefinitionsAndLabels.TryGetValue(_currentLexeme.Value, out currentLexeme))
        {
            // impossible !
            currentLexeme = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
        }

        foreach (var label in labels)
        {
            currentLexeme.labels.Add(new LexemeLabelAttribute(label.lang,label.label));    
        }
        
        _lexemesDefinitionsAndLabels[_currentLexeme.Value] = currentLexeme;
        return this;
    }

    public IAotLexemeBuilder<IN> WithModes(params string[] modes)
    {
        List<string> currentModes = new List<string>();
        if (!_modes.TryGetValue(_currentLexeme.Value, out currentModes))
        {
            currentModes = new List<string>();
        };
        currentModes.AddRange(modes);
        _modes[_currentLexeme.Value] = currentModes;
        return this;
    }
    

    public IAotLexemeBuilder<IN> OnChannel(int channel)
    {
        foreach (var definition in _lexemesDefinitionsAndLabels.Values.SelectMany(x => x.definitions))
        {
            definition.Channel = channel;
        }
        return this;
    }

    public IAotLexemeBuilder<IN> PopMode()
    {
        _modePopers.Add(_currentLexeme.Value);
        return this;
    }

    public IAotLexemeBuilder<IN> PushToMode(string targetMode)
    {
        _modePushers[_currentLexeme.Value] = targetMode;
        return this;
    }

    #endregion
}