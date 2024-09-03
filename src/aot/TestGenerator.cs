using aot.lexer;
using aot.parser;
using cslyGenerator;
using sly.lexer;
using sly.lexer.fsm;

namespace aot;

[ParserGenerator(typeof(AotLexer), typeof(AotParser), typeof(double))]
public partial class TestGenerator  : AbstractParserGenerator<AotLexer>
{
    public virtual Action<AotLexer, LexemeAttribute, GenericLexer<AotLexer>> UseTokenExtensions()
    {
        return null;
    }
    
    public virtual LexerPostProcess<AotLexer> UseTokenPostProcessor()
    {
        return (List<Token<AotLexer>> tokens) =>
        {
            return tokens;
        };
    }   
}



//
//
// public partial class TestGenerator {
//
//     
//
//     public void GetParser() {
//         Console.WriteLine("get parser for >TestGenerator<");
//     }
//     
//     public IAotLexerBuilder<AotLexer> GetLexer() {
//         var builder = AotLexerBuilder<AotLexer>.NewBuilder();
//         builder.IgnoreWhiteSpace(true);
//         builder.IgnoreKeywordCase(true);
//         builder.IsIndentationAware(false);
//         builder.UseWhiteSpaces(new[]{' ','\t'});
//         builder.IgnoreEol(true);
//         builder.UseIndentations("\t");
//         builder.Regex(AotLexer.PATTERN , "$-$");
//         builder.Double(AotLexer.DOUBLE );
//         builder.AlphaId(AotLexer.IDENTIFIER );
//         builder.Sugar(AotLexer.PLUS , "+");
//         builder.Sugar(AotLexer.INCREMENT , "++");
//         builder.Sugar(AotLexer.MINUS , "-");
//         builder.Sugar(AotLexer.TIMES , "*");
//         builder.Sugar(AotLexer.DIVIDE , "/");
//         builder.Sugar(AotLexer.LPAREN , "(");
//         builder.Sugar(AotLexer.RPAREN , ")");
//         builder.Sugar(AotLexer.FACTORIAL , "!");
//         builder.Sugar(AotLexer.SQUARE , "²");
//         builder.SingleLineComment(AotLexer.SINGLELINECOMMENT , "//");
//         builder.MultiLineComment(AotLexer.MULTILINECOMMENT , "/*", "*/");
//         ;
//         builder.UseLexerPostProcessor(UseTokenPostProcessor());
//         builder.UseExtensionBuilder(UseTokenExtensions());
//         return builder;
//     }
//}