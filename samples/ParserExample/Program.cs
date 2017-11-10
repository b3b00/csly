using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using jsonparser.JsonModel;
using jsonparser;
using System.Diagnostics;
using csly.whileLang.parser;
using csly.whileLang.model;
using sly.parser;
using csly.whileLang.interpreter;
using sly.lexer.fsm;

namespace ParserExample
{

    public enum TokenType
    {
        [Lexeme("a")]
        a = 1,
        [Lexeme("b")]
        b = 2,
        [Lexeme("c")]
        c = 3,
        [Lexeme("z")]
        z = 26,
        [Lexeme("r")]
        r = 21,
        [Lexeme("[ \\t]+",true)]
        WS = 100,
        [Lexeme("[\\r\\n]+",true,true)]
        EOL = 101
    }


    

   

    class Program
    {

        [Production("R : A b c ")]
        [Production("R : Rec b c ")]
        public static object R(List<object> args)
        {
            string result = "R(";
            result += args[0].ToString() + ",";
            result += (args[1] as Token<TokenType>).Value + ",";
            result += (args[2] as Token<TokenType>).Value;
            result += ")";
            return result;
        }

        [Production("A : a ")]
        [Production("A : z ")]
        public static object A(List<object> args)
        {
            string result = "A(";
            result += (args[0] as Token<TokenType>).Value;
            result += ")";
            return result;
        }

        [Production("Rec : r Rec ")]
        [Production("Rec :  ")]
        public static object Rec(List<object> args)
        {
            if (args.Count == 2)
            {
                
                string r = "Rec(" + (args[0] as Token<TokenType>).Value + "," + args[1].ToString() + ")";
                return r;
                ;
            }
            else
            {
                return "_";
                ;
            }
        }


        static void TestFactorial()
        {

            WhileParser whileParser = new WhileParser();
            ParserBuilder<WhileToken, WhileAST> builder = new ParserBuilder<WhileToken, WhileAST>();
            var Parser = builder.BuildParser(whileParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
            ;

            string program = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    ("; 
        program += "print \"r=\".r;\n";
            program += "r := r * i;\n";
            program += "print \"r=\".r;\n";
            program += "print \"i=\".i;\n";
            program += "i := i + 1 ))";
            ParseResult<WhileToken, WhileAST> result = Parser.Result.Parse(program);
            Interpreter interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result);
            ;
        }


        static void testLexerBuilder()
        {
            var builder = new FSMLexerBuilder<JsonToken, JsonToken>();


            // conf
            builder.IgnoreWS()
                .WhiteSpace(' ')
                .WhiteSpace('\t')
                .IgnoreEOL()
                .UseNixEOL();

            // start machine definition
            builder.Mark("start");



            // string literal
            builder.Transition('\"', JsonToken.STRING)
                .Mark("in_string")
                .ExceptTransitionTo(new char[] { '\"', '\\' }, "in_string", JsonToken.STRING)
                .Transition('\\',JsonToken.STRING)
                .Mark("escape")
                .AnyTransitionTo(' ',"in_string",JsonToken.STRING)
                .Transition('\"', JsonToken.STRING)
                .End(JsonToken.STRING)
                .Mark("string_end")
                .CallBack((FSMMatch<JsonToken> match) => {
                    match.Result.Value = match.Result.Value.ToUpper();
                    return match;
            } );

            // accolades
            builder.GoTo("start")
                .Transition('{')
                .End(JsonToken.ACCG);

            builder.GoTo("start")
                .Transition('}')
                .End(JsonToken.ACCD);

            // corchets
            builder.GoTo("start")
                .Transition('[')
                .End(JsonToken.CROG);

            builder.GoTo("start")
                .Transition(']')
                .End(JsonToken.CROD);

            // 2 points
            builder.GoTo("start")
                .Transition(':')
                .End(JsonToken.COLON);

            // comma
            builder.GoTo("start")
                .Transition(',')
                .End(JsonToken.COMMA);

            //numeric
            builder.GoTo("start")
            .RangeTransition('0', '9', JsonToken.INT, JsonToken.DOUBLE)
            .Mark("in_int")
            .RangeTransitionTo('0', '9', "in_int", JsonToken.INT, JsonToken.DOUBLE)
            .End(JsonToken.INT)
            .Transition('.', JsonToken.DOUBLE)
            .Mark("start_double")
            .RangeTransition('0', '9', JsonToken.INT, JsonToken.INT, JsonToken.DOUBLE)
            .Mark("in_double")
            .RangeTransitionTo('0', '9', "in_double", JsonToken.INT, JsonToken.DOUBLE)
            .End(JsonToken.DOUBLE);


            string code = "{\n\"d\" : 42.42 ,\n\"i\" : 42 ,\n\"s\" : \"quarante-deux\",\n\"s2\":\"a\\\"b\"\n}";
            //code = File.ReadAllText("test.json");
            var lex = builder.Fsm;
            var r = lex.Run(code,0);
            string total = "";
            while (r.IsSuccess)
            {
                string msg = $"{r.Result.TokenID} : {r.Result.Value} @{r.Result.Position}";
                total += msg + "\n";
                Console.WriteLine(msg);
                r = lex.Run(code);
            }

            
        }

        

        static void testGenericLexerWhile()
        {
           

            var sw = new Stopwatch();
           
            string source = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    ( 
    r := r * i;
    print r;
    print i;
    i := i + 1 )
)";

            WhileParser wp = new WhileParser();
            sw.Reset();
            sw.Start();
            ParserBuilder<WhileToken, WhileAST> wbuilder = new ParserBuilder<WhileToken, WhileAST>();
            var buildResult = wbuilder.BuildParser(wp, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
            var parser = buildResult.Result;
            var r = parser.Parse(source);
            sw.Stop();
            Console.WriteLine($"regex parser : {sw.ElapsedMilliseconds} ms");
            if (!r.IsError)
            {
                var interpreter = new Interpreter();
                var ctx = interpreter.Interprete(r.Result);
                ;

            }
            else
            {
                r.Errors.ForEach(e => Console.WriteLine(e.ToString()));
            }

            sw.Reset();
            sw.Start();
            WhileParserGeneric wpg = new WhileParserGeneric();
            ParserBuilder <WhileTokenGeneric, WhileAST > wbuilderGen = new ParserBuilder<WhileTokenGeneric, WhileAST>();
            var buildResultgen = wbuilderGen.BuildParser(wpg, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
            var parserGen = buildResult.Result;
            var rGen = parser.Parse(source);
            sw.Stop();
            Console.WriteLine($"generic parser : {sw.ElapsedMilliseconds} ms");
            if (!rGen.IsError)
            {
                var interpreter = new Interpreter();
                var ctx = interpreter.Interprete(rGen.Result);
                ;
                
            }
            else
            {
                rGen.Errors.ForEach(e => Console.WriteLine(e.ToString()));
            }


            ;
        }

        static void testGenericLexerJson()
        {


            var sw = new Stopwatch();

            string source =File.ReadAllText("test.json");

            EbnfJsonParser wp = new EbnfJsonParser();
            sw.Reset();
            sw.Start();
            ParserBuilder<JsonToken, JSon> wbuilder = new ParserBuilder<JsonToken, JSon>();
            var buildResult = wbuilder.BuildParser(wp, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            var parser = buildResult.Result;
            var r = parser.Parse(source);
            sw.Stop();
            Console.WriteLine($"json regex parser : {sw.ElapsedMilliseconds} ms");
            if (r.IsError)            
            {
                r.Errors.ForEach(e => Console.WriteLine(e.ToString()));
            }

            
            sw.Reset();
            sw.Start();
            wbuilder = new ParserBuilder<JsonToken, JSon>();
            buildResult = wbuilder.BuildParser(wp, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            parser = buildResult.Result;
            parser.Lexer = new JSONLexer();
            r = parser.Parse(source);
            Console.WriteLine($"json hard coded lexer : {sw.ElapsedMilliseconds} ms");
            sw.Stop();

            sw.Reset();
            sw.Start();
            EbnfJsonGenericParser wpg = new EbnfJsonGenericParser();
            ParserBuilder<JsonTokenGeneric, JSon> wbuilderGen = new ParserBuilder<JsonTokenGeneric, JSon>();
            var buildResultgen = wbuilderGen.BuildParser(wpg, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            var parserGen = buildResult.Result;
            var rGen = parser.Parse(source);
            sw.Stop();
            Console.WriteLine($"json generic parser : {sw.ElapsedMilliseconds} ms");
            if (rGen.IsError)
            {
                rGen.Errors.ForEach(e => Console.WriteLine(e.ToString()));
            }


            ;
        }

        static void testJSONLexer()
        {
            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(new JSONParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");

            string source = "{ \"k\" : 1;\"k2\" : 1.1;\"k3\" : null;\"k4\" : false}";
            source = File.ReadAllText("test.json");
            JSONLexer lexer = new JSONLexer();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var tokens = lexer.Tokenize(source);
            sw.Stop();
            Console.WriteLine($"hard coded lexer {tokens.Count()} tokens in {sw.ElapsedMilliseconds}ms");
            var sw2 = new Stopwatch();
            int start = DateTime.Now.Millisecond;
            sw2.Start();                        
            tokens = parser.Result.Lexer.Tokenize(source).ToList();
            sw2.Stop();
            int end = DateTime.Now.Millisecond;
            Console.WriteLine($"old lexer {tokens.Count()} tokens in {sw2.ElapsedMilliseconds}ms / {end-start}ms");


            ;
        }

        static void Main(string[] args)
        {

            //ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();
            ////Parser<JsonToken, JSon> parser = builder.BuildParser(new EbnfJsonParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            //Lexer<JsonToken> lexer = (Lexer<JsonToken>)LexerBuilder.BuildLexer<JsonToken>();
            //Stopwatch sw = new Stopwatch();

            //sw.Start();
            //string json = File.ReadAllText("test.json");
            //var result = lexer.Tokenize(json).ToList();
            //sw.Stop();
            //long milli = sw.ElapsedMilliseconds;
            //Console.WriteLine($"wo/ optim : {milli} ms");
            //sw.Reset();
            //sw.Start();
            //json = File.ReadAllText("test.json");
            //result = lexer.Tokenize(json).ToList();
            //sw.Stop();
            //milli = sw.ElapsedMilliseconds;
            //Console.WriteLine($"w/ optim : {milli} ms");
            //TestFactorial();
            //testLexerBuilder();


            //testJSONLexer();
            //testGenericLexerJSON();

            //testGenericLexerWhile();
            testGenericLexerJson();
            Console.WriteLine("so what ?");

            ;

        }
    }
}


