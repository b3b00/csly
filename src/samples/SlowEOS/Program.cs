using System.Collections.Immutable;
using System.Diagnostics;
using sly.parser;
using sly.parser.generator;

namespace SlowEOS;

public class Program
{

    private static Parser<SlowOnBadParseEosToken, object> GetParser(object instance)
    {
        ParserBuilder<SlowOnBadParseEosToken, object> builder = new ParserBuilder<SlowOnBadParseEosToken, object>("en");
        
        var buildBaseLine = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        if (buildBaseLine.IsOk)
        {
            return buildBaseLine.Result;
        }
        else
        {
            buildBaseLine.Errors.ForEach(e => Console.WriteLine(e.Message));
            return null;
        }
    }

    private static long Test(Parser<SlowOnBadParseEosToken, object> parser, string source)
    {
        Stopwatch chrono = new Stopwatch();
        chrono.Start();
        var parsed = parser.Parse(source);
        chrono.Stop();
        if (parsed.IsError)
        {
            parsed.Errors.ForEach(x => Console.WriteLine(x.ErrorMessage));
        }
        return chrono.ElapsedMilliseconds;
    } 
    
    public static void Main(string[] args)
    {
        string source = "FUNCTIONCALL([Identifier]";

        var baseLine = GetParser(new SlowOnBadParseEos());
        var reducedPrecedences = GetParser(new SlowOnBadParseEosReducedPrecedences());
        // for (int i = 0; i < 1000; i++)
        // {
            var baseTime = Test(baseLine, source);    
        // }
        
        var reducedTime = Test(reducedPrecedences, source);
        var delta = (((double)reducedTime - (double)baseTime) / (double)baseTime)*100; 
        Console.WriteLine($"base : {baseTime} ms, reduced : {reducedTime} ms. delta:{(delta>0 ? "+" : "")}{delta:##.00}%");
    }
}