using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.i18n;

namespace sly.lexer;

public class AotLexerBuilder<T> :  IAotLexerBuilder<T> where T : struct
{
    
    
    private Dictionary<T, (List<LexemeAttribute>, List<LexemeLabelAttribute>)> _lexemes;

    private IList<CommentAttribute> _comments = new List<CommentAttribute>();

    private T? _currentLexeme;
    private IList<string> _explicitTokens;

    public static IAotLexerBuilder<A> NewBuilder<A>() where A : struct
    {
        return new AotLexerBuilder<A>();
    }

    private AotLexerBuilder()
    {
        _lexemes = new Dictionary<T, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
    }

    private void Add(T tokenId, LexemeAttribute lexeme, LexemeLabelAttribute label)
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
    
    public IAotLexerBuilder<T> Double(T tokenId, string decimalDelimiter = ".", int channel = Channels.Main)
    {
        Add(tokenId, new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) , null);
        return this;
    }

    public IAotLexerBuilder<T> Integer(T tokenId, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Int, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexerBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token), null);
        return this;
    }
    
    public IAotLexerBuilder<T> Keyword(T tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, parameters:token) ,null);
        return this;
    }

    public IAotLexerBuilder<T> AlphaNumId(T tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) , null);
        return this;
    }

    public IAotLexerBuilder<T> Labeled(string lang, string label)
    {
        if (_currentLexeme == null)
        {
            throw new InvalidOperationException("no lexeme is currently defined");
        }
        AddLabel(lang, label);
        return this;
    }
    
    public IAotLexerBuilder<T> SingleLineComment(T tokenId, string start)
    {
        _comments.Add(new SingleLineCommentAttribute(start)); // TODO AOT : how is tokenId used ?
        return this;
    }

    public IAotLexerBuilder<T> MultiLineComment(T tokenId, string start, string end)
    {
        _comments.Add(new MultiLineCommentAttribute(start,end)); // TODO AOT : how is tokenId used ?
        return this;
    }

    public IAotLexerBuilder<T> WithExplicitTokens(IList<string> explicitTokens = null)
    {
        _explicitTokens = explicitTokens;
        return this;
    }
    
    public ILexer<T> Build()
    {
        BuildResult<ILexer<T>> result = new BuildResult<ILexer<T>>();
        var lexerResult = LexerBuilder.BuildGenericSubLexers<T>(_lexemes, null, result, "en",_explicitTokens);
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