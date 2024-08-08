

using aot.lexer;
using aot.parser;


// testing lexer builder 

var builder = new TestAotLexerBuilder();
var lexerBuilder = builder.FluentInitializeCenericLexerForLexerTest();
if (lexerBuilder != null)
{
    var lexer = lexerBuilder.Build();
    string source = "2 + 2 * ( 3 / 8) PLUS 42.42 100!";
    Console.WriteLine($"tokenize >{source}<");
    if (lexer.IsError)
    {
        foreach (var error in lexer.Errors)
        {
            Console.WriteLine(error);
        }

        return;
    }
    var lexingResult = lexer.Result.Tokenize(source);
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

// testing parser builder
var pBuilder = new TestAotParserBuilder();
var b = pBuilder.FluentInitializeCenericLexer();
if (b.IsError)
{
    foreach (var error in b.Errors)
    {
        Console.WriteLine(error);
    }

    return;
}
var r = b.Result.Parse("2 + 2 * 2");
if (r.IsOk)
{
    Console.WriteLine($"parse OK : {r.Result}");
}
else
{
    foreach (var error in r.Errors)
    {
        Console.WriteLine(error.ErrorMessage);
    }
}