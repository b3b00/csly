// See https://aka.ms/new-console-template for more information

using System;
using csly.whileLang.interpreter;
using SharpFileSystem.FileSystems;


namespace generatortester;
public partial class Program
{
    public static void Main(string[] args)
    {
        GeneratorForm form = new GeneratorForm();
        
        Run(form);
    }

    private static void Run(GeneratorForm form)
    {
        while (true)
        {
            form.Ask();
            EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(typeof(Program).Assembly);
            var source = form.Sample switch
            {
                "counter" => fs.ReadAllText("/samples/counter.while"),
                "factorial" => fs.ReadAllText("/samples/factorial.while"),
                "fibonacci" => fs.ReadAllText("/samples/fibonacci.while"),
                "quit" => "quit",
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
                Console.Error.WriteLine($"Sample {form.Sample} not found");
            }
        }
    }
}