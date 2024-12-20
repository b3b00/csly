// See https://aka.ms/new-console-template for more information

using sly.lexer;
using sly.lexer.fluent;

namespace aot;

public class Program
{
    public static void Main(string[] args)
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Int(FluentToken.INT)
            .Double(FluentToken.DOUBLE)
            .Hexa(FluentToken.HEXA,"0x")
            .Date(FluentToken.DATE,DateFormat.YYYYMMDD,'-')
            .String(FluentToken.STRING)
            .MultiLineComment(FluentToken.COMMENT,"/*","*/").OnChannel(Channels.Main)
            .Keyword(FluentToken.HELLO, "hello")
            .Keyword(FluentToken.WORLD, "world")
            .Sugar(FluentToken.START_ISLAND, ">>>").PushToMode("island")
            .Sugar(FluentToken.CURLY_CLOSE, "<<<").WithModes("island").PopMode()
            .Sugar(FluentToken.COMMA, ",")
            .UpTo(FluentToken.UPTO, "<<<").WithModes("island")
            .Build("en");


        if (lexer.IsError)
        {
            Console.WriteLine(" !!!! ERROR WHILE BUILDING LEXER");
            lexer.Errors.ForEach(e => Console.WriteLine(e));
            return;
        }

        var graph = (lexer.Result as GenericLexer<FluentToken>).FSMBuilder.Fsm.ToGraphViz();
        File.WriteAllText("c:/tmp/fluent.dot", graph);
        var tokenized = lexer.Result.Tokenize(@"
2024-12-20
0xb3b00
hello, world
identifier
/*
comment
content
*/
""string""
1
2.3
>>>
island content
<<< 
");
        if (tokenized.IsOk)
        {
            Console.WriteLine(" !!!!! YES !!!!!!!");
            foreach (var token in tokenized.Tokens.MainTokens())
            {
                Console.WriteLine(token.ToString());
            }
        }
        else
        {
            Console.WriteLine(" !!!!! OH NO !!!!!!!");
            Console.WriteLine(tokenized.Error);
        }


    }
}