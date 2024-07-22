using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.i18n;
using sly.lexer;

namespace ParserExample.aot;



public class AotParserAndlexer
{

    private LexemeAttribute BuildLexeme(GenericToken generic, int channel = 0, params string[] parameters)
    {
        return new LexemeAttribute(generic, channel, parameters);
        return null;
    }

    public void InitializeCenericLexer()
    {

        var lexemes = new Dictionary<AotExpressionToken, (List<LexemeAttribute>,List<LexemeLabelAttribute>)>()
        {
            {
                AotExpressionToken.DOUBLE,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.Double, Channels.Main) }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.PLUS,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "+") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.MINUS,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "-") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.TIMES,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "*") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.DIVIDE,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "/") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.LPAREN,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "(") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.RPAREN,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, ")") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.FACTORIAL,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "!") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.INCREMENT,
                (new List<LexemeAttribute>() { new LexemeAttribute(GenericToken.SugarToken, Channels.Main, "++") }, new List<LexemeLabelAttribute>())
            },
            {
                AotExpressionToken.IDENTIFIER,
                (new List<LexemeAttribute>()
                    { new LexemeAttribute(GenericToken.Identifier, IdentifierType.Alpha, channel: Channels.Main) }, new List<LexemeLabelAttribute>())
            }
        };
        //public static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(IDictionary<IN, List<LexemeAttribute>> attributes,
        //Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder, BuildResult<ILexer<IN>> result, string lang,
        //IList<string> explicitTokens = null)
        BuildResult<ILexer<AotExpressionToken>> result = new BuildResult<ILexer<AotExpressionToken>>();
        var lexerResult = LexerBuilder.BuildGenericSubLexers<AotExpressionToken>(lexemes, null, result, "en");
        if (lexerResult.IsOk)
        {
            Console.WriteLine("lexer build : OK");
            var lexingResult = lexerResult.Result.Tokenize("2 + 2 * ( 3 / 8)");
            if (lexerResult.IsOk)
            {
                Console.WriteLine("lexing OK");
                foreach (var token in lexingResult.Tokens)
                {
                    Console.WriteLine(token.ToString());
                }
            }
            else
            {

                Console.WriteLine($"lexing KO : {lexingResult.Error}");
            }
        }
        else
        {
            Console.WriteLine("lexer build KO");
            foreach (var error in lexerResult.Errors)
            {
                Console.WriteLine($"[{error.Code}] {error.Message}");
            }
        }
    }
}