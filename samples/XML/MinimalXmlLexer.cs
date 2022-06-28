using System;
using sly.lexer;

namespace XML
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
        [Mode("tag","pi")]
        EQUALS,
        
        [String]
        [Mode("tag","pi")]
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
}