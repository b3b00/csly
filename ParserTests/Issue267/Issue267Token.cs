using System;
using System.Collections.Generic;
using System.Text;

using sly.lexer;

namespace ParserTests.Issue267
{
    
    [CallBacks(typeof(Issue267TokensCallbacks))]
    public enum Issue267Token
    {
       [AlphaId]
       Identifier,
       
       [Keyword("declare")]
       Declare,
       
       [Sugar("=")]
       Equal,
       
       [Double]
       Double
       
    }
}
