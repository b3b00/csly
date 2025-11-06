// See https://aka.ms/new-console-template for more information

using System;
using csly.whileLang.interpreter;
using SharpFileSystem.FileSystems;


namespace generatortester;
public partial class Program
{
    public static void Main(string[] args)
    {
        Run();
    }

    private static void Run()
    {
        while (true)
        {
            Console.WriteLine("choose : ");
            Console.WriteLine("   1. while");
            Console.WriteLine("   2. factorial");
            Console.WriteLine("   3. fibonacci");
            Console.WriteLine("   4. quit");
            var choice = Console.ReadLine();
            while (choice != "1" && choice != "2" && choice != "3" &&  choice != "4")
            {
                Console.Write("Bad choice. Retry.");
                choice = Console.ReadLine();
            }
            EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(typeof(Program).Assembly);
            var source = choice switch
            {
                "1" => fs.ReadAllText("/samples/counter.while"),
                "2" => fs.ReadAllText("/samples/factorial.while"),
                "3" => fs.ReadAllText("/samples/fibonacci.while"),
                "4" => "quit",
                _ => null
            };


            if (source != null)
            {
                if (source == "quit")
                {
                    return;
                }

                WhileGenerator whiler = new WhileGenerator();
                var build = whiler.GetParser();
                if (build != null && build.IsOk)
                {
                    var parser = build.Result;
                    var parse = parser.Parse(source);
                    if (parse != null && parse.IsOk)
                    {
                        var ast = parse.Result;
                        var interpreter = new Interpreter();
                        var context = interpreter.Interprete(ast, false);
                        foreach (var variable in context.variables)
                        {
                            Console.WriteLine($"{variable.Key} = {variable.Value}");
                        }
                    }
                    else
                    {
                        parse.Errors.ForEach(e => Console.Error.WriteLine(e.ErrorMessage));
                    }
                }
                else
                {
                    build.Errors.ForEach(e => Console.Error.WriteLine(e.Message));
                }
            }
            else
            {
                Console.Error.WriteLine($"Sample {choice} not found");
            }
        }
    }
}