// See https://aka.ms/new-console-template for more information


using aot.lexer;



var builder = new AotLexerBuilder();
var lexer = builder.FluentInitializeCenericLexer();
if (lexer != null)
{
    string source = "2 + 2 * ( 3 / 8)";
    Console.WriteLine($"tokenize >{source}<");    
    var lexingResult = lexer.Tokenize(source);
    if (lexingResult.IsOk)
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
