
using sly.parser.generator;
using System;

namespace csly.cli {
    

    public class Program {
        public static void Main(string[] args) {
            var builder = new ParserBuilder<CLIToken, object>();
            var instance = new CLIParser();

            var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, null,null);
            if (buildParser.IsOk)
            {
                var result = buildParser.Result.Parse("<< HERE COMES YOUR SOURCE");
                if (result.IsOk)
                {
                    Console.WriteLine(result.Result);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

            }
        }
    }
}
