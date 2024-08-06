using sly.buildresult;
using sly.i18n;
using sly.lexer;

namespace aot.lexer;



public interface ILexemeBuilder<T> where T : struct
{
    
    public ILexemeBuilder<T> Double(T tokenId, string decimalDelimiter = ".",
        int channel = Channels.Main);

    public ILexemeBuilder<T> Integer(T tokenId, int channel = Channels.Main);

    public ILexemeBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main);

    public ILexemeBuilder<T> AlphaNumId(T tokenId);
    
    ILexer<T> Build();
}

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

    public ILexemeBuilder<T> Double(T tokenId, string decimalDelimiter = ".", int channel = Channels.Main)
    {
        Lexemes[tokenId] =
            (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.Double, channel:Channels.Main, parameters:decimalDelimiter) },
                new List<LexemeLabelAttribute>());
        return this;
    }

    public ILexemeBuilder<T> Integer(T tokenId, int channel = Channels.Main)
    {
        Lexemes[tokenId] =
            (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.Int, channel:Channels.Main) },
                new List<LexemeLabelAttribute>());
        return this;
    }

    public ILexemeBuilder<T> Sugar(T tokenId, string token, int channel = Channels.Main)
    {
        Lexemes[tokenId] =
            (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, channel:Channels.Main, parameters:token) },
                new List<LexemeLabelAttribute>());
        return this;
    }

    public ILexemeBuilder<T> AlphaNumId(T tokenId)
    {
        Lexemes[tokenId] =
            (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.Identifier,IdentifierType.AlphaNumeric, channel:Channels.Main) },
                new List<LexemeLabelAttribute>());
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

public class AotLexerBuilder
{


    private LexemeAttribute BuildLexeme(GenericToken generic, int channel = 0, params string[] parameters)
    {
        return new LexemeAttribute(generic, channel, parameters);
        return null;
    }


    public ILexer<AotLexer> FluentInitializeCenericLexer()
    {
        var builder = Builder<AotLexer>.NewBuilder<AotLexer>();
        var lexer = builder.Double(AotLexer.DOUBLE)
            .Sugar(AotLexer.PLUS, "+")
            .Sugar(AotLexer.MINUS, "-")
            .Sugar(AotLexer.TIMES, "*")
            .Sugar(AotLexer.DIVIDE, "/")
            .Sugar(AotLexer.LPAREN, "(")
            .Sugar(AotLexer.RPAREN, ")")
            .Sugar(AotLexer.FACTORIAL, "!")
            .Sugar(AotLexer.INCREMENT, "++")
            .AlphaNumId(AotLexer.IDENTIFIER)
            .Build();
        return lexer;
    }


    public ILexer<AotLexer> InitializeCenericLexer()
    {

        
        
        var lexemes = new Dictionary<AotLexer, (List<LexemeAttribute>, List<LexemeLabelAttribute>)>()
        {
            {
                AotLexer.DOUBLE,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.Double, Channels.Main) },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.PLUS,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "+") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.MINUS,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "-") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.TIMES,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "*") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.DIVIDE,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "/") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.LPAREN,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "(") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.RPAREN,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, ")") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.FACTORIAL,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "!") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.INCREMENT,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "++") },
                    new List<LexemeLabelAttribute>())
            },
            {
                AotLexer.IDENTIFIER,
                (new List<LexemeAttribute>()
                        { new LexemeAttribute(GenericToken.Identifier, IdentifierType.Alpha, channel: Channels.Main) },
                    new List<LexemeLabelAttribute>())
            }
        };
        //public static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(IDictionary<IN, List<LexemeAttribute>> attributes,
        //Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder, BuildResult<ILexer<IN>> result, string lang,
        //IList<string> explicitTokens = null)
        BuildResult<ILexer<AotLexer>> result = new BuildResult<ILexer<AotLexer>>();
        var lexerResult = LexerBuilder.BuildGenericSubLexers<AotLexer>(lexemes, null, result, "en");
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