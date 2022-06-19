

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
        #region sea
        [Sugar("<")]
        [Mode()]
        [Push("tag")]
        OPEN,
        
        [Sugar("<!--")]
        [Mode()]
        [Push("comment")]
        OPEN_COMMENT,
        
        [AllExcept("<")]
        [Mode()]
        CONTENT,
        
        


        #endregion
        
        #region in comment
        
        [AllExcept("-->")]
        [Mode("comment")]
        COMMENT_CONTENT,
        
        [Sugar("-->")]
        [Mode("comment")]
        [Pop]
        CLOSE_COMMENT,
        
        #endregion
        
        #region in tag
        
        [AlphaId]
        [Mode("tag")]
        ID,
        
        [Sugar("/")]
        [Mode("tag")]
        SLASH,
        
        [Sugar("=")]
        [Mode("tag")]
        EQUALS,
        
        [String]
        [Mode("tag")]
        VALUE,
        
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
<!-- this is a comment -->");
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
                MinimalXmlLexer.OPEN_COMMENT,
                MinimalXmlLexer.COMMENT_CONTENT,
                MinimalXmlLexer.CLOSE_COMMENT
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