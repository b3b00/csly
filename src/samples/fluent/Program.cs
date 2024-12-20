// See https://aka.ms/new-console-template for more information

using sly.lexer;
using sly.lexer.fluent;
using sly.lexer.fsm;

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
            .UpTo(FluentToken.UPTO, "<<<").WithModes("island")
            .Sugar(FluentToken.END_ISLAND, "<<<").WithModes("island").PopMode()
            .Sugar(FluentToken.COMMA, ",")
            .Extension(FluentToken.EXTENSION)
            .UseExtensionBuilder((
                (FluentToken token, LexemeAttribute attribute, GenericLexer<FluentToken> genericLexer) =>
                {
                    var fsmBuilder = genericLexer.FSMBuilder;
                    NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) => 
                    {
                        match.Properties[GenericLexer<FluentToken>.DerivedToken] = FluentToken.EXTENSION;
                        return match;
                    };
                    
                    fsmBuilder.GoTo(GenericLexer<FluentToken>.start)
                        .ConstantTransition("...")
                        .End(GenericToken.Extension) // mark as ending node 
                        .CallBack(callback); 
                }))
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
Lorem ipsum dolor 
sit amet, consectetur.
*/
""string""
1
2.3
>>>
island content with int : 1, double :2.3, hexa 0xFFF, keyword : hello  
<<< 
");
        if (tokenized.IsOk)
        {
            Console.WriteLine(" !!!!! YES !!!!!!!");
            foreach (var token in tokenized.Tokens.AllExceptWhiteSpaces)
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