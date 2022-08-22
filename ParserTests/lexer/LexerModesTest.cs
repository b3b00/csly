

using System.Collections.Generic;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using XML;
using Xunit;

namespace ParserTests.lexer
{
   
    
    
    public class LexerModesTest
    {
        [Fact]
        public static void TestLexerModes()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MinimalXmlLexer>>());
            Check.That(lexerRes.IsError).IsFalse();
            var result = lexerRes.Result.Tokenize(@"hello
<tag attr=""value"">inner text</tag>
<!-- this is a comment -->
<? PI attr=""test""?>");
            Check.That(result.IsOk).IsTrue();
            var expectedTokens = new List<MinimalXmlLexer>()
            {
                MinimalXmlLexer.CONTENT,
                MinimalXmlLexer.OPEN,
                MinimalXmlLexer.ID,
                MinimalXmlLexer.ID,
                MinimalXmlLexer.EQUALS,
                MinimalXmlLexer.VALUE,
                MinimalXmlLexer.CLOSE,
                MinimalXmlLexer.CONTENT,
                MinimalXmlLexer.OPEN,
                MinimalXmlLexer.SLASH,
                MinimalXmlLexer.ID,
                MinimalXmlLexer.CLOSE,
                MinimalXmlLexer.COMMENT,
                MinimalXmlLexer.OPEN_PI,
                MinimalXmlLexer.ID,
                MinimalXmlLexer.ID,
                MinimalXmlLexer.EQUALS,
                MinimalXmlLexer.VALUE,
                MinimalXmlLexer.CLOSE_PI
            };
            var tokens = result.Tokens.Tokens;
            Check.That(expectedTokens).CountIs(tokens.Count-1);

            Check.That(tokens.Extracting("TokenID")).Contains(expectedTokens);
            
        }

        [Fact]
        public void TestXmlParserWithLexerModes()
        {
            ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
            var parser = new MinimalXmlParser();
            var r = builder.BuildParser(parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "document");
            Check.That(r.IsError).IsFalse();
            var pr = r.Result.Parse(@"
<?xml version=""1.0""?>
<!-- starting doc -->
<root name=""root"">
    <autoInner name=""autoinner1""/>
    <inner name=""inner"">
         <?PI name=""pi""?> 
        <innerinner name=""innerinner"">
            inner inner content
        </innerinner>
    </inner>                      
</root>
");
            Check.That(pr.IsOk).IsTrue();
            Check.That(pr.Result).IsNotNull();
            Check.That(pr.Result).IsNotEmpty();
            
        }
    }
}