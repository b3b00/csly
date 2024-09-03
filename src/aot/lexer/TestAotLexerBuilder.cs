using sly.buildresult;
using sly.i18n;
using sly.lexer;

namespace aot.lexer;

public class TestAotLexerBuilder
{

    /// <summary>
    /// this should be the code to generate
    /// </summary>
    /// <returns></returns>
    public IAotLexerBuilder<AotLexer> FluentInitializeCenericLexerForParserTest()
    {
        var builder = AotLexerBuilder<AotLexer>.NewBuilder();
        var lexerBuilder = builder.Double(AotLexer.DOUBLE)
            .Sugar(AotLexer.PLUS, "+")
            .Keyword(AotLexer.PLUS, "PLUS")
            .Label(AotLexer.PLUS, "en", "sum")
            .Label(AotLexer.PLUS, "fr", "somme")
            .Sugar(AotLexer.MINUS, "-")
            .Sugar(AotLexer.TIMES, "*")
            .Sugar(AotLexer.DIVIDE, "/")
            .Sugar(AotLexer.LPAREN, "(")
            .Sugar(AotLexer.RPAREN, ")")
            .Sugar(AotLexer.INCREMENT, "++")
            .Sugar(AotLexer.SQUARE, "²")
            .AlphaNumId(AotLexer.IDENTIFIER);
            
            
        return lexerBuilder;
    }
    
    public IAotLexerBuilder<AotLexer> FluentInitializeCenericLexerForLexerTest()
    {
        var builder = AotLexerBuilder<AotLexer>.NewBuilder();
        var lexerBuilder = builder.Double(AotLexer.DOUBLE)
            .Sugar(AotLexer.PLUS, "+")
            .Keyword(AotLexer.PLUS, "PLUS")
            .Label(AotLexer.PLUS,"en", "sum")
            .Label(AotLexer.PLUS,"fr", "somme")
            .Sugar(AotLexer.MINUS, "-")
            .Sugar(AotLexer.TIMES, "*")
            .Sugar(AotLexer.DIVIDE, "/")
            .Sugar(AotLexer.LPAREN, "(")
            .Sugar(AotLexer.RPAREN, ")")
            //.Sugar(AotLexer.FACTORIAL, "!")
            .Sugar(AotLexer.INCREMENT, "++")
            .AlphaNumId(AotLexer.IDENTIFIER)
            .WithExplicitTokens(new List<string>() { "!" });
            
        return lexerBuilder;
    }


    public ILexer<AotLexer>? InitializeCenericLexer()
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