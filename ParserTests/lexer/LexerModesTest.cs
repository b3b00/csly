

using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.False(lexerRes.IsError);
            var result = lexerRes.Result.Tokenize(@"hello
<tag attr=""value"">inner text</tag>
<!-- this is a comment -->
<? PI attr=""test""?>");
            Assert.True(result.IsOk);
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
            Assert.Equal(expectedTokens.Count,tokens.Count-1);
            for (int i = 0; i < expectedTokens.Count; i++)
            {
                Assert.Equal(expectedTokens[i],tokens[i].TokenID);
            }
        }

        [Fact]
        public void TestXmlParserWithLexerModes()
        {
            ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
            var parser = new MinimalXmlParser();
            var r = builder.BuildParser(parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "document");
            Assert.False(r.IsError);
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
            Assert.True(pr.IsOk);
            Assert.NotNull(pr.Result);
            Assert.NotEmpty(pr.Result);
            
        }
    }
}