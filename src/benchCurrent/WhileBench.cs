using System;
using System.IO;
using bench.json;
using bench.json.model;
using benchCurrent.backtrack;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using sly.parser;
using sly.parser.generator;


namespace benchCurrent
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class WhileBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
            }
        }

        private Parser<IndentedWhileTokenGeneric,WhileAST> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            content = @"
r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
	r2:=1
	i2:=1
	while i < 11 do 
		r2 := r2 * i2
		print r2
		print i2
		i2 := i2 + 1
	b2 := false 
	if i2 == 589 then
		r3:=1
		i3:=1
		while i3 < 11 do 
			r3 := r3 * i3
			print r3
			print i3
			i3 := i3 + 1
		b3 := false 
		if i3 == 589 then
			r4:=1
			i4:=1
			while i4 < 11 do 
				r4 := r4 * i4
				print r4
				print i4
				i4 := i4 + 1
			b4 := false 
			if i4 == 589 then
				r5:=1
				i5:=1
				while i5 < 11 do 
					r5 := r5 * i5
					print r5
					print i5
					i5 := i5 + 1
				b5 := false 
				if i5 == 589 then
					r6:=1
					i6:=1
					while i6 < 11 do 
						r6 := r6 * i6
						print r6
						print i6
						i6 := i6 + 1
					b6 := false 
					if i6 == 589 then			
						r7:=1
						i7:=1
						while i7 < 11 do 
							r7 := r7 * i7
							print r7
							print i7
							i7 := i7 + 1
						b7 := false 
						if i7 == 589 then
							print ""youpalou""

";
            
            var backTrackParser = new IndentedWhileParserGeneric();
            var builder = new ParserBuilder<IndentedWhileTokenGeneric, WhileAST>();
            
            var result = builder.BuildParser(backTrackParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            
            if (result.IsError)
            {
                result.Errors.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("parser ok");
                BenchedParser = result.Result;
            }
        }

        [Params(true,false)]
        public bool Memoize { get; set; }
        
        [Params(true,false)]
        public bool Broaden { get; set; }
        
        [Benchmark]
        
        public void TestWhile()
        {
            if (BenchedParser == null)
            {
                Console.WriteLine("parser is null");
            }
            else
            {
                BenchedParser.Configuration.UseMemoization = Memoize;
                BenchedParser.Configuration.BroadenTokenWindow = Broaden;
                var ignored = BenchedParser.Parse(content);
            }
        }



    }

}
