using cpg.parser.parsgenerator;
using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System;
using System.Collections.Generic;


namespace ParserExample.cs
{
    public enum TokenType
    {
        a = 1,
        b = 2,
        c = 3,
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
            return lexer;
        }

        [Reduction("R : A b c ")]
        public static object R(List<object> args)
        {
            return null;
        }

        [Reduction("A : a ")]
        public static object A(List<object> args)
        {
            return null;
        }

        static void Main(string[] args)
        {            
            object parser = ParserGenerator.BuildParser< TokenType>(typeof(Program), ParserType.LL, "R");
            ;
        }
    }
}