

using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.lexer
{
    [Lexer()]
    [Modes("one","two")]
    public enum MinimalXmlLexer
    {
        [Sugar("<")]
        [Mode()]
        [Push("tag")]
        OPEN,
        
        [AlphaId]
        [Mode("tag")]
        ID,
        
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
        
        [AllExcept(">")]
        CONTENT
        
        
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