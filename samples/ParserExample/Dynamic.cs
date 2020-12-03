using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using expressionparser;
using ParserTests;
using Sigil;
using simpleExpressionParser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserExample
{
    
    #region UpperParser
    
    public enum DynamicLexer
    {
        EOF = 0,
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumeric)] ID = 1,
        
        [SubParser(typeof(SubParser),typeof(SubToken),typeof(string),"main")]
        [SingleLineIsland("``",channel:Channels.Islands)] MYISLANDSINGLE = 2,
        
        [SubParser(typeof(SubParser),typeof(SubToken),typeof(string),"main")]
        [MultiLineIsland("`","`",channel:Channels.Islands)] MYISLANDMULTI = 3
    }

    public class DynamicParser
    {
        [Production("main : id *")]
        public Ast Main(List<Ast> ids)
        {
            return new IdentifierList(ids);
        }

        [Production("id : ID")]
        public Ast SimpleId(Token<DynamicLexer> token)
        {
            // get previous token in channel 2 (COMMENT)
            var previous = token.Previous(Channels.Islands);
            double? island = null;
            // previous token may not be a comment so we have to check if not null
            if (previous != null && (previous.TokenID == DynamicLexer.MYISLANDMULTI ||
                                     previous.TokenID == DynamicLexer.MYISLANDSINGLE))
            {
                var result = (previous.ParsedValue as ParseResult<ExpressionToken, double>);
                island  = result.IsOk ? result?.Result : null;
            }

            return new AstCommentIdentifier(token.Value, island);
        }
    }
    
    public interface Ast
    {
        
    }

    public class IdentifierList : Ast
    {

        public List<AstCommentIdentifier> Ids;
        public IdentifierList(List<Ast> ids)
        {
            Ids = ids.Cast<AstCommentIdentifier>().ToList();
        }
    }
    
    public class AstCommentIdentifier : Ast
    {
        public string Name;

        public double? Value;

        public bool IsCommented => Value.HasValue;

        public AstCommentIdentifier(string name)
        {
            Name = name;
        }

        public AstCommentIdentifier(string name, double? value) : this(name)
        {
            Value = value;
        }
    }

    #endregion
    
    #region subparser
    
    public enum SubToken
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID = 1,

        [Lexeme(GenericToken.KeyWord, "aaa")] A = 2,
        
        [Lexeme(GenericToken.KeyWord, "bbb")] B = 3,
        
        [Lexeme(GenericToken.KeyWord, "ccc")] C = 4

    }

    public class SubParser
    {

        [Production("main : (a b c)*")]
        public string Main(List<Group<SubToken, string>> groups)
        {
            var groupsStrings = groups.Select(group => $"({group.Value(0)},{group.Value(1)},{group.Value(2)})");
            var grps = string.Join(", ", groupsStrings);
            return $"[{grps}]";
        }

        [Production("a : A")]
        public string A(Token<SubToken> a)
        {
            return a.Value;
        }
        
        [Production("b : B")]
        public string B(Token<SubToken> b)
        {
            return b.Value;
        }
        
        [Production("c : C")]
        public string C(Token<SubToken> c)
        {
            return c.Value;
        }
        
        [Production("d : D")]
        public string D(Token<SubToken> d)
        {
            return d.Value;
        }
        /*
         
         main : (a b c)*
         a : A
         b : B
         c : C
         d : D
         
         
         */
    }
    
    
    #endregion

    public class Dynamic
    {


        static void Create()
        {
            ParserBuilder<DynamicLexer,Ast> builder = new ParserBuilder<DynamicLexer, Ast>();
            builder.BuildParser(new DynamicParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT,"main", null);
        }


        public static void Test()
        {
            var lexerType = typeof(DynamicLexer);
            var visitorType = typeof(DynamicParser);
            var astType = typeof(Ast);
            string startingRule = "main";
            string source = @"
id1
`` 1 + 1 -2 + 2
id2
`(9 / 3) * 1`
id3
";
            

            var parser = CreateParserMethod(visitorType, lexerType, astType, startingRule, out var parseMethod);

            var parsed = parser.parseMethod.Invoke(parser.parser, new object[] {source, startingRule});

        }

        private static (object parser, MethodInfo parseMethod) CreateParserMethod(Type visitorType, Type lexerType, Type astType, string startingRule,
            out MethodInfo parseMethod)
        {
            var parserInstance = Activator.CreateInstance(visitorType);


            var buildertype = typeof(ParserBuilder<,>);
            var builderT = buildertype.MakeGenericType(lexerType, astType);
            var builder = Activator.CreateInstance(builderT);


            var method = builderT.GetMethod("BuildParser");
            var parserResult = method.Invoke(builder,
                new Object[] {parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule, null});
            ;
            var realParserResultType = parserResult.GetType();

            var prop = realParserResultType.GetProperty("Result");
            var parser = prop.GetValue(parserResult);
            var realParserType = parser.GetType();

            parseMethod = realParserType.GetMethod("Parse", new Type[] {typeof(string), typeof(string)});
            ;
            return (parser, parseMethod);
        }

        public static void TestSubParsers()
        {
            var parserInstance =  new DynamicParser();
            var Builder = new ParserBuilder<DynamicLexer, Ast>();
            var buildResult = Builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main");
            if (buildResult.IsOk && buildResult.Result != null)
            {
                var parser = buildResult.Result;
                string source = @"
id1
`` 1 + 1 -2 + 2
id2
`(9 / 3) * 1`
id3
";
                var result = parser.Parse(source);
            }
            else
            {
                foreach (var error in buildResult.Errors)
                {
                    Console.WriteLine(error.Level+" - "+error.Message);
                }
            }
            
        }
    }
}