// See https://aka.ms/new-console-template for more information


using handExpressions.jsonparser;
using testgen;


namespace testgen;

    

public class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TestEbnfJsonGenericParserGenerator generator = new TestEbnfJsonGenericParserGenerator(new TestEbnfJsonGenericParser());
        //var j = generator.Parse("root", "{\"hello\":\"world\"}");

        var instance = new TestEbnfJsonGenericParser();

        // var generated = new GeneratedTestEbnfJsonGenericParser(instance);
        
         // var j = generated.Parse("root", "{\"hello\":\"world\"}");
        
        // Console.WriteLine(j.ToString());
        
        
    }

}