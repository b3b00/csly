using cpg.parser.parsgenerator.parser;
using jsonparser;
using lexer;
using parser.parsergenerator.generator;
using System;
using System.Collections.Generic;

namespace ParserExample
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
            Lexer<JsonToken>  lex = JSONParser.BuildJsonLexer(new Lexer<JsonToken>());
            Parser<JsonToken> yacc = ParserBuilder.BuildParser<JsonToken>(typeof(JSONParser), ParserType.LL_RECURSIVE_DESCENT, "root");
            object result = yacc.Parse("[1,null,{},true,42.58]");
            ;

            result = yacc.Parse("\"hello\" \"world!\"");
            ;
        }
    }
}
