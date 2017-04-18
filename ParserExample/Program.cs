using cpg.parser.parsgenerator;
using cpg.parser.parsgenerator.parser;
using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System.Linq;
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
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.b, "b"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.c, "c"));
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
            Parser<TokenType> parser = ParserGenerator.BuildParser<TokenType>(typeof(Program), ParserType.LL, "R");
            Lexer<TokenType> lexer = BuildLexer();
            List<Token<TokenType>> tokens = lexer.Tokenize("a b c").ToList<Token<TokenType>>();
            parser.Parse(tokens)
            ;
        }
    }
}