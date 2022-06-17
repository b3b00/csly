

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
        
        [AllExcept("<")]
        [Mode()]
        CONTENT,
        
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
        }
    }
}