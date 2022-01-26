using System;
using System.Collections.Generic;
using System.Text;

using sly.lexer;

namespace ParserTests.Issue263
{
    public enum Issue263Token
    {
        [Lexeme("\\(")]
        LPARA,

        [Lexeme("\\)")]
        RPARA,

        [Lexeme("a")]
        IDENTIFIER,

        [Lexeme("\\[")]
        LBRAC,

        [Lexeme("\\]")]
        RBRAC,
    }
}
