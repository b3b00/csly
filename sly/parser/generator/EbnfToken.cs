using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace sly.parser.generator
{
    public enum EbnfToken
    {

        [Lexeme("^[A-Za-z][A-Za-z0-9_]*") ]
        IDENTIFIER = 1,
        [Lexeme(":")]
        COLON = 2,
        [Lexeme("\\*")]
        ZEROORMORE = 3,
        [Lexeme("\\+")]
        ONEORMORE = 4,
        [Lexeme("[ \\t]+",true)]
        WS = 5,    
        [LexemeAttribute("^\\?")]
        OPTION = 6,
        [LexemeAttribute("^\\[d\\]")]
        DISCARD = 7,
        [Lexeme("[\\n\\r]+",true,true)]
        EOL = 8

    }

}
