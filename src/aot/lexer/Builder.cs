using sly.buildresult;
using sly.i18n;
using sly.lexer;

namespace aot.lexer;

public class Builder<T> :  ILexemeBuilder<T> where T : struct
{
    
    
    private Dictionary<T, (List<LexemeAttribute>, List<LexemeLabelAttribute>)> Lexemes =
        new Dictionary<T, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
    public static ILexemeBuilder<A> NewBuilder<A>() where A : struct
    {
        return new Builder<A>();
    }

    private Builder()
    {
        Lexemes = new Dictionary<T, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>();
    }

    private void Add(T tokenId, LexemeAttribute lexeme, LexemeLabelAttribute label)
    {
        (List<LexemeAttribute> tokens, List<LexemeLabelAttribute> labels) lexemes = (null,null);
        if (!Lexemes.TryGetValue(tokenId, out lexemes))
        {
            lexemes = (new List<LexemeAttribute>(), new List<LexemeLabelAttribute>());
        }
        lexemes.tokens.Add(lexeme);
        lexemes.labels.Add(label);
        Lexemes[tokenId] = lexemes;
    }
    
    public ILexemeBuilder<T> Double(T tokenId, string decimalDelimiter = ".", int channel = Channels.Main)
    {
        Add(tokenId, new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) , null);
        return this;
    }

    public ILexemeBuilder<T> Integer(T tokenId, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Int, channel:Channels.Main) , null);
        return this;
    }

    public ILexemeBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token), null);
        return this;
    }
    
    public ILexemeBuilder<T> Keyword(T tokenId, string token, int channel = Channels.Main)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.KeyWord, channel:Channels.Main, parameters:token) ,null);
        return this;
    }

    public ILexemeBuilder<T> AlphaNumId(T tokenId)
    {
        Add(tokenId,  new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) , null);
        return this;
    }

    public ILexer<T> Build()
    {
        BuildResult<ILexer<T>> result = new BuildResult<ILexer<T>>();
        var lexerResult = LexerBuilder.BuildGenericSubLexers<T>(Lexemes, null, result, "en");
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