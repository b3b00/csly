using System.Linq;
using expressionparser;
using NFluent;
using simpleExpressionParser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Discussion468
{
    public class Discussion468Tests
    {



        private static void CheckId(ILexer<Discussion468Lexer> lexer,string source, bool isOk)
        {
            
            var lexingResult = lexer.Tokenize(source);
            if (isOk)
            {
                Check.That(lexingResult).IsOkLexing();
                var tokens = lexingResult.Tokens;
                Check.That(tokens).IsNotNull();
                Check.That(tokens).CountIs(2);
                var idToken = tokens[0];
                Check.That(idToken.TokenID).IsEqualTo(Discussion468Lexer.IDENTIFIER);
                Check.That(idToken.Value).IsEqualTo(source);
            }
            else
            {
                Check.That(lexingResult).Not.IsOkLexing();
            }
        }

        [Fact]
        public static void Test468()
        {
            var buildResult = LexerBuilder.BuildLexer<Discussion468Lexer>();
            Check.That(buildResult).IsOk();
            var lexer = buildResult.Result;
            Check.That(lexer).IsNotNull();
            CheckId(lexer,"_:word1234",true);
            CheckId(lexer,"word_1234.otherword",true);
            CheckId(lexer,"1_:word1234",false);
            
        }
        
        private static void CheckId(ILexer<Discussion468WithExtensionLexer> lexer,string source, bool isOk)
        {
            
            var lexingResult = lexer.Tokenize(source);
            if (isOk)
            {
                Check.That(lexingResult).IsOkLexing();
                var tokens = lexingResult.Tokens;
                Check.That(tokens).IsNotNull();
                Check.That(tokens).CountIs(2);
                var idToken = tokens[0];
                Check.That(idToken.TokenID).IsEqualTo(Discussion468WithExtensionLexer.IDENTIFIER);
                Check.That(idToken.Value).IsEqualTo(source);
            }
            else
            {
                Check.That(lexingResult).Not.IsOkLexing();
            }
        }
        
        [Fact]
        public static void Test468WithExtension()
        {
            var buildResult = LexerBuilder.BuildLexer<Discussion468WithExtensionLexer>(extensionBuilder:Discussion468Extension.AddExtension);
            Check.That(buildResult).IsOk();
            var lexer = buildResult.Result;
            Check.That(lexer).IsNotNull();
            CheckId(lexer,"_:word1234",true);
            CheckId(lexer,"word_1234.otherword",true);
            CheckId(lexer,"Test1",true);
            CheckId(lexer,"Test12",true);
            CheckId(lexer,"1_:word1234",false);
            
        }
        
       
    }
}