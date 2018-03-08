using sly.parser;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer
{
    public class LexerException : Exception
    {

        public LexicalError Error { get; set; }

        public LexerException(LexicalError error)
        {
            Error = error;
        }

    }
}
