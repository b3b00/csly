using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;
using System;
using System.Linq;
using System.Reflection;

namespace ParserExample
{

    public enum TokenType
    {
        [Description("one")]
        a = 1,
        [Description("two")]
        b = 2,
        c = 3,
        z = 26,
        r = 21,
        WS = 100,
        EOL = 101
    }


    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute : Attribute
    {

        public string Description { get; set; }

        public bool IsSkippable { get; set; }

        public bool IsEnding { get; set; }

        public DescriptionAttribute(string description)
        {
            Description = description;            
        }
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


            DescriptionAttribute attr = TokenType.a.GetAttributeOfType<DescriptionAttribute>();
            string desc = attr.Description;

            ILexer<ExpressionToken> lexer = LexerBuilder.BuildLexer<ExpressionToken>();


            ExpressionParser exprParser = new ExpressionParser();

            ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
            var p = builder.BuildParser(exprParser, ParserType.LL_RECURSIVE_DESCENT, "expression");
            var r = p.Parse("1", "expression");


            //RuleParser<JsonToken> ruleparser = new RuleParser<JsonToken>();
            //ParserBuilder builder = new ParserBuilder();

            //Parser<EbnfToken,GrammarNode<JsonToken>> yacc = builder.BuildParser<EbnfToken,GrammarNode<JsonToken>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule");

            //var r = yacc.Parse("test : CROG INT CROD");
            //;

        }
    }
}
