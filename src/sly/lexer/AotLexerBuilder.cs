using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.i18n;

namespace sly.lexer;

public class AotLexerBuilder<IN> :  IAotLexerBuilder<IN> where IN : struct
{
    
    
    private Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)> _lexemes;

    private IList<CommentAttribute> _comments = new List<CommentAttribute>();

    private IN? _currentLexeme;
    private IList<string> _explicitTokens;

    public static IAotLexerBuilder<A> NewBuilder<A>() where A : struct
    {
        return new AotLexerBuilder<A>();
    }

    private AotLexerBuilder()
    {
        _lexemes = new Dictionary<IN, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
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
    
    public IAotLexerBuilder<IN> SingleLineComment(IN tokenId, string start)
    {
        _comments.Add(new SingleLineCommentAttribute(start)); // TODO AOT : how is tokenId used ?
        return this;
    }

    public IAotLexerBuilder<IN> MultiLineComment(IN tokenId, string start, string end)
    {
        _comments.Add(new MultiLineCommentAttribute(start,end)); // TODO AOT : how is tokenId used ?
        return this;
    }

    public IAotLexerBuilder<IN> WithExplicitTokens(IList<string> explicitTokens = null)
    {
        _explicitTokens = explicitTokens;
        return this;
    }
    
    public ILexer<IN> Build()
    {
        BuildResult<ILexer<IN>> result = new BuildResult<ILexer<IN>>();
        var lexerResult = LexerBuilder.BuildGenericSubLexers<IN>(_lexemes, null, result, "en",_explicitTokens);
        if (lexerResult.IsOk)
        {
            return lexerResult.Result;
        }
        else
        {
            Console.WriteLine("lexer build KO");
            foreach (var error in lexerResult.Errors)
            {
                Console.WriteLine($"[{error.Code}] {error.Message}");
            }
        }

        return null;
    }
}