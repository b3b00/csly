

using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.lexer
{
    [Lexer()]
    [Modes("one","two")]
    public enum MinimalXmlLexer
    {
        NOT_A_TOKEN = 0,
        
        
        #region sea
        [Sugar("<")]
        [Mode()]
        [Push("tag")]
        OPEN,
        
        [AllExcept("<")]
        CONTENT,
        
        [Sugar("<?")]
        [Mode]
        [Push("pi")]
        OPEN_PI,


        #endregion
        
        
        [MultiLineComment("<!--","-->",channel:Channels.Main)]
        [Mode]
        COMMENT,
        
        
        #region pi
        
        #endregion
        
        
        #region in tag
        
        [AlphaId]
        [Mode("tag")]
        [Mode("pi")]
        ID,
        
        [Sugar("/")]
        [Mode("tag")]
        SLASH,
        
        [Sugar("=")]
        [Mode("tag")]
        [Mode("pi")]
        EQUALS,
        
        [String]
        [Mode("tag")]
        [Mode("pi")]
        VALUE,
        
        [Sugar("?>")]
        [Mode("pi")]
        [Pop]
        CLOSE_PI,
        
        
        
        [Sugar(">")]
        [Mode("tag")]
        [Pop]
        CLOSE,
        
        #endregion
        
        
        
    }
    
    
    public class LexerModesTest
    {
        [Fact]
        public static void Test()
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
    }
}