using Chrono.Ext;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using ParserTests.aot;
using sly.parser;
using sly.parser.generator;

namespace aot;

public class WhileBench
{

    string program = @"
a:=0
while a < 10 do 
	print a
	a := a +1
if a > 145 then
	print a
	skip
	a := a *478
	if a > 125874 then
		a := 1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20
		b := true
		print ""bololo""
	else
		b := false
		print ""youpi""
else
	print b
";
    
    public void BenchAot(ChronoExts chrono)
    {
        AotIndentedWhileParserBuilder builder = new AotIndentedWhileParserBuilder();

        var b = builder.BuildAotWhileParser();
        var p = b.BuildParser();
        if (!p.IsOk)
        {
            Environment.Exit(12);
        }
        chrono.Tick("build");
        var parser = p.Result;
        Run(parser);
        chrono.Tick("run");
        Console.WriteLine(p.Result.Configuration.Dump());
    }

    public void BenchLegacy(ChronoExts chrono)
    {
	    var builder = new ParserBuilder<IndentedWhileTokenGeneric, WhileAST>();
	    var instance = new IndentedWhileParserGeneric();
	    var parser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
	    chrono.Tick("build");
	    Run(parser.Result);
	    chrono.Tick("run");
    }

    private void Run(Parser<IndentedWhileTokenGeneric, WhileAST> parser)
    {
	    for (int i = 0; i < 30_000; i++)
	    {
		    parser.Parse(program);
	    }
    }

    public void Bench(Action<ChronoExts> action)
    {
	    var chrono = new ChronoExts();
	    chrono.Start();
	    action(chrono);
	    chrono.Stop();
	    Console.WriteLine(chrono.ToString());
    }
    
}