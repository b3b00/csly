// See https://aka.ms/new-console-template for more information

using testgen;

public class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        //EbnfJsonGenericParserGenerator generator = new EbnfJsonGenericParserGenerator(new EbnfJsonGenericParser());
        //var j = generator.Parse("root", "{\"hello\":\"world\"}");

        var generated = new GeneratedEbnfJsonGenericParser(new EbnfJsonGenericParser());
        var j = generated.Parse("root", "{\"hello\":\"world\"}");
        
        Console.WriteLine(j.ToString());
    }

}