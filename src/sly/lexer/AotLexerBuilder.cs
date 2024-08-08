using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.i18n;

namespace sly.lexer;

public class AotLexerBuilder<IN> :  IAotLexerBuilder<IN> where IN : struct
{
    private bool _ignoreWS = true;

    private bool _ignoreEOL = true;

    private char[]_whiteSpaces;

    private bool _keyWordIgnoreCase = false;

    private bool _indentationAware = false;

    private string _indentation;
    
    private Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)> _lexemes;

    private Dictionary<IN, List<CommentAttribute>> _comments;

    private IN? _currentLexeme;
    private IList<string> _explicitTokens;

    
    
    public static IAotLexerBuilder<A> NewBuilder<A>() where A : struct
    {
        return new AotLexerBuilder<A>();
    }

    
    
    private AotLexerBuilder()
    {
        _lexemes = new Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
        _comments = new Dictionary<IN, List<CommentAttribute>>();
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
        _lexemes[tokenId] = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
    }
    
    private void Add(IN tokenId, LexemeAttribute lexeme, LexemeLabelAttribute label)
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
    }
    
    private void AddLabel(string lang, string label)
    {
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes = (null,null);
        if (_lexemes.TryGetValue(_currentLexeme.Value, out lexemes))
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

    public IAotLexerBuilder<IN> SingleLineComment(string start)
    {
        // TODO AOT
        return this;
    }
    
    public IAotLexerBuilder<IN> MultiLineComment(string start)
    {
        // TODO AOT
        return this;
    }
    
    public IAotLexerBuilder<IN> Double(IN tokenId, string decimalDelimiter = ".", int channel = Channels.Main)
    {
        Add(tokenId, new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) , null);
        return this;
    }

    public IAotLexerBuilder<IN> Integer(IN tokenId, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Int, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexerBuilder<IN> Sugar(IN tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token), null);
        return this;
    }
    
    public IAotLexerBuilder<IN> Keyword(IN tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, parameters:token) ,null);
        return this;
    }

    public IAotLexerBuilder<IN> AlphaNumId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexerBuilder<IN> AlphaId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Alpha, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexerBuilder<IN> AlphaNumDashId(IN tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumericDash, channel:Channels.Main) , null);
        return this;
    }
    
    public IAotLexerBuilder<IN> CustomId(IN tokenId, string start, string end)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.Custom, start, end, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexerBuilder<IN> Labeled(string lang, string label)
    {
        if (_currentLexeme == null)
        {
            throw new InvalidOperationException("no lexeme is currently defined");
        }
        AddLabel(lang, label);
        return this;
    }

    public IAotLexerBuilder<IN> String(IN tokenId, string delimiter ="\"", string escapeChar = "\\", int channel = Channels.Main)
    {
        Add(tokenId,new LexemeAttribute(GenericToken.String,channel, delimiter,escapeChar),null);
        return this;
    }
    
    public IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start, bool doNotIgnore = false, int channel = Channels.Comments)
    {
        AddComment(tokenId, new SingleLineCommentAttribute(start, doNotIgnore, channel));
        return this;
    }

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end, bool doNotIgnore = false, int channel = Channels.Comments)
    {
        AddComment(tokenId, new MultiLineCommentAttribute(start, end, doNotIgnore, channel));
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
    
    public BuildResult<ILexer<IN>> Build()
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
        var lexerResult = LexerBuilder.BuildLexer(r, null, "en", null, _explicitTokens, _lexemes, lexerConfig, _comments);
        return lexerResult;
    }
}