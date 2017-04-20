using cpg.parser.parsgenerator;
using cpg.parser.parsgenerator.parser;
using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ParserExample.cs
{
    public enum TokenType
    {
        a = 1,
        b = 2,
        c = 3,
        z = 26,
        r = 21,
        WS = 100,
        EOL = 101
    }



    class Program
    {

        public static Lexer<TokenType> BuildLexer()
        {
            Lexer<TokenType> lexer = new Lexer<TokenType>();
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.EOL, "[\\n\\r]+", true, true));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.a, "a"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.b, "b"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.c, "c"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.z, "z"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.r, "r"));
            return lexer;
        }

        [Reduction("R : A b c ")]
        [Reduction("R : Rec b c ")]
        public static object R(List<object> args)
        {
            string result = "R(";
            result += args[0].ToString() + ",";
            result += (args[1] as Token<TokenType>).Value + ",";
            result += (args[2] as Token<TokenType>).Value;
            result += ")";
            return result;
        }

        [Reduction("A : a ")]
        [Reduction("A : z ")]
        public static object A(List<object> args)
        {
            string result = "A(";
            result += (args[0] as Token<TokenType>).Value;
            result += ")";
            return result;
        }

        [Reduction("Rec : r Rec ")]
        [Reduction("Rec :  ")]
        public static object Rec(List<object> args)
        {
            if (args.Count == 2)
            {
                
                string r = "Rec("+(args[0] as Token<TokenType>).Value + "," +args[1].ToString()+")";
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
            //string content = "a b c";
            //Parser<TokenType> parser = ParserGenerator.BuildParser<TokenType>(typeof(Program), ParserType.LL, "R");
            //Lexer<TokenType> lexer = BuildLexer();
            //List<Token<TokenType>> tokens = lexer.Tokenize(content).ToList<Token<TokenType>>();
            //List<object> r = (List<object>)parser.Parse(tokens);
            //Console.WriteLine($"{content} => {r[0].ToString()}")
            //;
            //content = "z b c";
            //tokens = lexer.Tokenize("z b c").ToList<Token<TokenType>>();
            //r = (List<object>)parser.Parse(tokens);
            //Console.WriteLine($"{content} => {r[0].ToString()}");
            //;
            //content = "r r b c";
            //tokens = lexer.Tokenize(content).ToList<Token<TokenType>>();
            //r = (List<object>)parser.Parse(tokens);
            //Console.WriteLine($"{content} => {r[0].ToString()}");
            //;

            string json = "{\"int\" :42,\"str\":\"hello\",\"dbl\":42.42,\"vrai\":true,\"faux\":false}";
            json = "{\"int\" :42,\"str\":\"hello\",\"o\":{\"o1\":1,\"o2\":2,\"o3\":true}}";
            Lexer < JsonToken > lexer = JSONParser.BuildJsonLexer();
            IList<Token<JsonToken>> tokens = lexer.Tokenize(json).ToList<Token<JsonToken>>();
            Parser<JsonToken> parser = ParserGenerator.BuildParser<JsonToken>(typeof(JSONParser), ParserType.LL, "root");
            var res = parser.Parse(tokens);
            ;
        }
    }
}