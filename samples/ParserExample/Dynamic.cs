using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ParserTests;
using Sigil;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace ParserExample
{
    
    public enum DynamicLexer
    {
        EOF = 0,
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumeric)] ID = 1,
        [SingleLineIsland("``",channel:Channels.Islands)] MYISLANDSINGLE = 2,
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
            string island = null;
            // previous token may not be a comment so we have to check if not null
            if (previous != null && (previous.TokenID == DynamicLexer.MYISLANDMULTI ||
                                     previous.TokenID == DynamicLexer.MYISLANDSINGLE))
            {
                island = previous?.ParsedValue?.ToString();
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

        public string Comment;

        public bool IsCommented => !string.IsNullOrEmpty(Comment);

        public AstCommentIdentifier(string name)
        {
            Name = name;
        }

        public AstCommentIdentifier(string name, string comment) : this(name)
        {
            Comment = comment;
        }
    }


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

            var parserInstance = Activator.CreateInstance(visitorType);


            var buildertype = typeof(ParserBuilder<,>);
            var builderT = buildertype.MakeGenericType(lexerType, astType);
            var builder = Activator.CreateInstance(builderT);


            var method = builderT.GetMethod("BuildParser");
            var parserResult = method.Invoke(builder,
                new Object[] {parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main", null});
            ;
            var realParserResultType = parserResult.GetType();

            var prop = realParserResultType.GetProperty("Result");
            var parser = prop.GetValue(parserResult);
            var realParserType = parser.GetType();
            
            var parseMethod = realParserType.GetMethod("Parse", new Type[] {typeof(string), typeof(string)});
            ;
            
            var methods = realParserResultType.GetMethods();
            ;
            
            string source = @"
id1
`` single line island
id2
`multi
line
island`
id3
";

            var parsed = parseMethod.Invoke(parser, new object[] {source, "main"});
            ;




        }
    }
}