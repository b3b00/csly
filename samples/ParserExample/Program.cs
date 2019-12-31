using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using csly.whileLang.compiler;
using csly.whileLang.interpreter;
using csly.whileLang.model;
using csly.whileLang.parser;
using expressionparser;
using GenericLexerWithCallbacks;
using jsonparser;
using jsonparser.JsonModel;
using ParserTests;
using simpleExpressionParser;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.buildresult;
using sly.parser.generator.visitor;

namespace ParserExample
{
    public enum TokenType
    {
        [Lexeme("a")] a = 1,
        [Lexeme("b")] b = 2,
        [Lexeme("c")] c = 3,
        [Lexeme("z")] z = 26,
        [Lexeme("r")] r = 21,
        [Lexeme("[ \\t]+", true)] WS = 100,
        [Lexeme("[\\r\\n]+", true, true)] EOL = 101
    }


    public enum CharTokens {
        [Lexeme(GenericToken.Char,"'","\\")]
//        [Lexeme(GenericToken.Char,"|","\\")]
        MyChar,

//        [Lexeme(GenericToken.Char,"|","\\")]
//        OtherChar,
//
//        [Lexeme(GenericToken.String,"'","\\")]
//        MyString
    }

    
    internal class Program
    {
        [Production("R : A b c ")]
        [Production("R : Rec b c ")]
        public static object R(List<object> args)
        {
            var result = "R(";
            result += args[0] + ",";
            result += (args[1] as Token<TokenType>).Value + ",";
            result += (args[2] as Token<TokenType>).Value;
            result += ")";
            return result;
        }

        [Production("A : a ")]
        [Production("A : z ")]
        public static object A(List<object> args)
        {
            var result = "A(";
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
                var r = "Rec(" + (args[0] as Token<TokenType>).Value + "," + args[1] + ")";
                return r;
            }

            return "_";
        }


        private static void TestFactorial()
        {
            var whileParser = new WhileParser();
            var builder = new ParserBuilder<WhileToken, WhileAST>();
            var Parser = builder.BuildParser(whileParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");

            var program = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    (";
            program += "\nprint \"r=\".r;\n";
            program += "r := r * i;\n";
            program += "print \"r=\".r;\n";
            program += "print \"i=\".i;\n";
            program += "i := i + 1 \n);\n";
            program += "return r)\n";
            var result = Parser.Result.Parse(program);
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result);

            var compiler = new WhileCompiler();
            var code = compiler.TranspileToCSharp(program);
            var f = compiler.CompileToFunction(program);
        }


        private static void testLexerBuilder()
        {
            var builder = new FSMLexerBuilder<JsonToken>();


            // conf
            builder.IgnoreWS()
                .WhiteSpace(' ')
                .WhiteSpace('\t')
                .IgnoreEOL();

            // start machine definition
            builder.Mark("start");


            // string literal
            builder.Transition('\"')
                .Mark("in_string")
                .ExceptTransitionTo(new[] { '\"', '\\' }, "in_string")
                .Transition('\\')
                .Mark("escape")
                .AnyTransitionTo(' ', "in_string")
                .Transition('\"')
                .End(JsonToken.STRING)
                .Mark("string_end")
                .CallBack(match =>
                {
                    string upperVAlue = match.Result.Value.ToString().ToUpper();
                    match.Result.SpanValue = new ReadOnlyMemory<char>(upperVAlue.ToCharArray());
                    return match;
                });

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
                .RangeTransition('0', '9')
                .Mark("in_int")
                .RangeTransitionTo('0', '9', "in_int")
                .End(JsonToken.INT)
                .Transition('.')
                .Mark("start_double")
                .RangeTransition('0', '9')
                .Mark("in_double")
                .RangeTransitionTo('0', '9', "in_double")
                .End(JsonToken.DOUBLE);


            var code = "{\n\"d\" : 42.42 ,\n\"i\" : 42 ,\n\"s\" : \"quarante-deux\",\n\"s2\":\"a\\\"b\"\n}";
            //code = File.ReadAllText("test.json");
            var lex = builder.Fsm;
            var r = lex.Run(code, 0);
            var total = "";
            while (r.IsSuccess)
            {
                var msg = $"{r.Result.TokenID} : {r.Result.Value} @{r.Result.Position}";
                total += msg + "\n";
                Console.WriteLine(msg);
                r = lex.Run(code);
            }
        }


        private static void testGenericLexerWhile()
        {
            var sw = new Stopwatch();

            var source = @"
(
    r:=1;
    i:=1;
    while i < 11 DO 
    ( 
    r := r * i;
    PRINT r;
    print i;
    i := i + 1 )
)";


            sw.Reset();
            sw.Start();
            var wpg = new WhileParserGeneric();
            var wbuilderGen = new ParserBuilder<WhileTokenGeneric, WhileAST>();
            var buildResultgen = wbuilderGen.BuildParser(wpg, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
            var parserGen = buildResultgen.Result;
            var rGen = parserGen.Parse(source);
            sw.Stop();
            Console.WriteLine($"generic parser : {sw.ElapsedMilliseconds} ms");
            if (!rGen.IsError)
            {
                var interpreter = new Interpreter();
                var ctx = interpreter.Interprete(rGen.Result);
            }
            else
            {
                rGen.Errors.ForEach(e => Console.WriteLine(e.ToString()));
            }
        }

        private static void testGenericLexerJson()
        {
            var sw = new Stopwatch();

            var source = File.ReadAllText("test.json");

            var wp = new EbnfJsonParser();
            sw.Reset();
            sw.Start();
            var wbuilder = new ParserBuilder<JsonToken, JSon>();
            var buildResult = wbuilder.BuildParser(wp, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            var parser = buildResult.Result;
            var r = parser.Parse(source);
            sw.Stop();
            Console.WriteLine($"json regex parser : {sw.ElapsedMilliseconds} ms");
            if (r.IsError) r.Errors.ForEach(e => Console.WriteLine(e.ToString()));


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
            var wpg = new EbnfJsonGenericParser();
            var wbuilderGen = new ParserBuilder<JsonTokenGeneric, JSon>();
            var buildResultgen = wbuilderGen.BuildParser(wpg, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            var parserGen = buildResultgen.Result;
            var rGen = parserGen.Parse(source);
            sw.Stop();
            Console.WriteLine($"json generic parser : {sw.ElapsedMilliseconds} ms");
            if (rGen.IsError) rGen.Errors.ForEach(e => Console.WriteLine(e.ToString()));
        }

        private static void testJSONLexer()
        {
            var builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(new JSONParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");

            var source = "{ \"k\" : 1;\"k2\" : 1.1;\"k3\" : null;\"k4\" : false}";
            //source = File.ReadAllText("test.json");
            var lexer = new JSONLexer();
            var sw = new Stopwatch();
            sw.Start();
            var lexresult = lexer.Tokenize(source);
            if (lexresult.IsOk)
            {
                var tokens = lexresult.Tokens;
                sw.Stop();
                Console.WriteLine($"hard coded lexer {tokens.Count()} tokens in {sw.ElapsedMilliseconds}ms");
                var sw2 = new Stopwatch();
                var start = DateTime.Now.Millisecond;
                sw2.Start();
                lexresult = parser.Result.Lexer.Tokenize(source);
                if (lexresult.IsOk)
                {
                    tokens = lexresult.Tokens;
                    sw2.Stop();
                    var end = DateTime.Now.Millisecond;
                    Console.WriteLine(
                        $"old lexer {tokens.Count()} tokens in {sw2.ElapsedMilliseconds}ms / {end - start}ms");
                }
            }
        }


        private static void testErrors()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var parser = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root").Result;


            var source = @"{
    'one': 1,
    'bug':{,}
}".Replace("'", "\"");
            var r = parser.Parse(source);

            var isError = r.IsError; // true
            var root = r.Result; // null;
            var errors = r.Errors; // !null & count > 0
            var error = errors[0] as UnexpectedTokenSyntaxError<JsonToken>; // 
            var token = error.UnexpectedToken.TokenID; // comma
            var line = error.Line; // 3
            var column = error.Column; // 12
        }

        private static void TestRuleParser()
        {
            Console.WriteLine("hum hum...");
            var parserInstance = new RuleParser<EbnfToken>();
            var builder = new ParserBuilder<EbnfToken, IClause<EbnfToken>>();
            var r = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "rule");

            var parser = r.Result;
            var rule = parser.Parse("a ( b ) ", "clauses");
        }


        public static BuildResult<Parser<ExpressionToken, int>> buildSimpleExpressionParserWithContext()
        {


            var StartingRule = $"{typeof(SimpleExpressionParserWithContext).Name}_expressions";
            var parserInstance = new SimpleExpressionParserWithContext();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            return Parser;
        }

        public static void TestContextualParser()
        {
            var buildResult = buildSimpleExpressionParserWithContext();
            if (buildResult.IsError)
            {
                buildResult.Errors.ForEach(e =>
                {
                    Console.WriteLine(e.Level + " - " + e.Message);
                });
                return;
            }
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a", new Dictionary<string, int> { { "a", 2 } });
            Console.WriteLine($"result : ok:>{res.IsOk}< value:>{res.Result}<");
        }

        public static void TestTokenCallBacks()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CallbackTokens>>());
            if (!res.IsError)
            {
                var lexer = res.Result as GenericLexer<CallbackTokens>;
                CallBacksBuilder.BuildCallbacks(lexer);

                var r = lexer.Tokenize("aaa bbb");
                if (r.IsOk)
                {
                    var tokens = r.Tokens;
                    foreach (var token in tokens)
                    {
                        Console.WriteLine($"{token.TokenID} - {token.Value}");
                    }
                }
            }

        }

        public static void test104()
        {
            EBNFTests tests = new EBNFTests();
            tests.TestGroupSyntaxOptionIsNone();

        }

        public static void testJSON()
        {
            try {

                var instance = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var buildResult = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            // if (buildResult.IsOk)
            // {
            //     Console.WriteLine("parser built.");
            //     var parser = buildResult.Result;
            //     var content = File.ReadAllText("test.json");
            //     Console.WriteLine("test.json read.");
            //     var jsonResult = parser.Parse(content);
            //     Console.WriteLine("json parse done.");
            //     if (jsonResult.IsOk)
            //     {
            //         Console.WriteLine("YES !");
            //     }
            //     else
            //     {
            //         Console.WriteLine("Ooh no !");
            //     }
            //     Console.WriteLine("Done.");
            //
            // }
            // else
            // {
            //     buildResult.Errors.ForEach(e => Console.WriteLine(e.Message));
            // }
            }
            catch(Exception e) {
                Console.WriteLine($"ERROR {e.Message} : \n {e.StackTrace}");
            }

        }

        private static void TestGraphViz()
        {
            var StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            var result = parser.Result.Parse("2 + 2 * 3");
            var tree = result.SyntaxTree;
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExpressionToken>();
            var root = graphviz.VisitTree(tree);
            string graph = graphviz.Graph.Compile();
            File.Delete("c:\\temp\\tree.dot");
            File.AppendAllText("c:\\temp\\tree.dot", graph);
        }

        private static void benchLexer()
        {
            var content = File.ReadAllText("test.json");

            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<JsonTokenGeneric>>());
            ILexer<JsonTokenGeneric> BenchedLexer = null;
            if (lexerRes != null)
            {
                BenchedLexer = lexerRes.Result;
                BenchedLexer.Tokenize(content);
            }
        }
        
        private static void TestChars()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CharTokens>>());
            if (res.IsOk)
            {
                var lexer = res.Result as GenericLexer<CharTokens>;

                var dump = lexer.ToString();
                var graph = lexer.ToGraphViz();
                Console.WriteLine(graph);
                var source = "'\\''";
                Console.WriteLine(source);
                var res2 = lexer.Tokenize(source);
                Console.WriteLine($"{res2.IsOk} - {res2.Tokens[0].Value}");
                var sourceU = "'\\u0066'";
                var res3 = lexer.Tokenize(sourceU);
                Console.WriteLine($"{res3.IsOk} - {res3.Tokens[0].Value}");
            }
            else
            {
                var errors = string.Join('\n',res.Errors.Select(e => e.Level + " - " + e.Message).ToList());
                Console.WriteLine("error building lexer : ");
                Console.WriteLine(errors);
            }
        }

        private static void TestGrammarParser()
        {
            string productionRule = "clauses : clause (COMMA [D] clause)*";
            var ruleparser = new RuleParser<TestGrammarToken>();
            var builder = new ParserBuilder<EbnfTokenGeneric, GrammarNode<TestGrammarToken>>();
            var grammarParser = builder.BuildParser(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule").Result;
            var result = grammarParser.Parse(productionRule);
            grammarParser.Lexer.ResetLexer();
            //(grammarParser.Lexer as GenericLexer<TestGrammarToken>).ResetLexer();
            Console.WriteLine($"alors ? {string.Join('\n',result.Errors.Select(e => e.ErrorMessage))}");
            result = grammarParser.Parse(productionRule);
            Console.WriteLine($"alors ? {string.Join('\n',result.Errors.Select(e => e.ErrorMessage))}");
            ;
            
            Console.WriteLine("starting");
            ErroneousGrammar parserInstance = new ErroneousGrammar();
            Console.WriteLine("new instance");

            var builder2 = new ParserBuilder<TestGrammarToken, object>();
            Console.WriteLine("builder");

            var Parser = builder.BuildParser(parserInstance,ParserType.EBNF_LL_RECURSIVE_DESCENT,"rule");
            Console.WriteLine($"built : {Parser.IsOk}");

            
        }


        public static void TestScript()
        {
            var parserInstance = new ScriptParser();
            var builder = new ParserBuilder<ScriptToken, object>();
            var parserBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "test");
            if (parserBuild.IsOk)
            {
                var parser = parserBuild.Result;
                string ok1 = @"|B|study(""Name or something"", overlay)|E|";
                string ok2 = "|B|test(close, 123, open)|E|";
                string kw = "|B|test(kw=123)|E|";
                string ko1 = "|B|test2(a, b, c=100)|E|";
                string ko2 = "|B|plotshape(data, style=shapexcross)|E|";

                // var r = parser.Parse(ok1);
                // r = parser.Parse(ok2);
                // r = parser.Parse(kw);
                // var r = parser.Parse("a, b, c=100", "fun_actual_args2");
                // var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                
                var r = parser.Parse(ko1);
                var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                var root = graphviz.VisitTree(r.SyntaxTree);
                var graph = graphviz.Graph.Compile();
                r = parser.Parse(ko2);
            }
            else
            {
                foreach (var e in parserBuild.Errors)
                {
                    Console.WriteLine(e.Level+ " - " + e.Message);
                }
            }
        }
        
        public static void TestScriptRight()
        {
            var parserInstance = new ScriptParserRight();
            var builder = new ParserBuilder<ScriptToken, object>();
            var parserBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "test");
            if (parserBuild.IsOk)
            {
                var parser = parserBuild.Result;
                string ok1 = @"|B|study(""Name or something"", overlay)|E|";
                string ok2 = "|B|test(close, 123, open)|E|";
                string kw = "|B|test(kw=123)|E|";
                string ko1 = "|B|test2(a, b, c=100, d=200)|E|";
                string ko2 = "|B|plotshape(data, style=shapexcross)|E|";
                string ko3 = "|B|plotshape(data = default, style=shapexcross)|E|";
                string badmixko = "|B|plotshape(data = default, t, y=20)|E|";

                // var r = parser.Parse(ok1);
                // r = parser.Parse(ok2);
                // r = parser.Parse(kw);
                var r = parser.Parse("a, b, c=100, d= 200", "fun_actual_args");
                var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                var root1 = graphviz.VisitTree(r.SyntaxTree);
                var graph1 =  graphviz.Graph.Compile();
                
                r = parser.Parse(ko1);
                graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                var root = graphviz.VisitTree(r.SyntaxTree);
                var graph = graphviz.Graph.Compile();
                r = parser.Parse(ko3);
                graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                root = graphviz.VisitTree(r.SyntaxTree);
                graph = graphviz.Graph.Compile();
                r = parser.Parse(badmixko);
                graphviz = new GraphVizEBNFSyntaxTreeVisitor<ScriptToken>();
                root = graphviz.VisitTree(r.SyntaxTree);
                graph = graphviz.Graph.Compile();
            }
            else
            {
                foreach (var e in parserBuild.Errors)
                {
                    Console.WriteLine(e.Level+ " - " + e.Message);
                }
            }
        }

        private static void Main(string[] args)
        {
            //TestContextualParser();
            //TestTokenCallBacks();
            //test104();
            //testJSON();
           //TestGrammarParser();
            // TestGraphViz();

            // TestGraphViz();
//            TestChars();
            TestScript();
            TestScriptRight();
        }
    }

    public enum TestGrammarToken
    {
        [Lexeme(GenericToken.SugarToken,",")]
        COMMA = 1
    }

    public class ErroneousGrammar
    {
        [Production("clauses : clause (COMMA [D] clause)*")]

            public object test()
            {
                return null;
            }    
    }
    
}