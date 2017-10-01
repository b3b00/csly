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

        static void Main(string[] args)
        {

            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();
            //Parser<JsonToken, JSon> parser = builder.BuildParser(new EbnfJsonParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Lexer<JsonToken> lexer = (Lexer<JsonToken>)LexerBuilder.BuildLexer<JsonToken>();
            Stopwatch sw = new Stopwatch();

            sw.Start();
            string json = File.ReadAllText("test.json");
            var result = lexer.Tokenize(json).ToList();
            sw.Stop();
            long milli = sw.ElapsedMilliseconds;
            Console.WriteLine($"wo/ optim : {milli} ms");
            sw.Reset();
            sw.Start();
            json = File.ReadAllText("test.json");
            result = lexer.Tokenize(json).ToList();
            sw.Stop();
            milli = sw.ElapsedMilliseconds;
            Console.WriteLine($"w/ optim : {milli} ms");


            ;

        }
    }
}
