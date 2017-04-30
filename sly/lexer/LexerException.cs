using cpg.parser.parsgenerator.parser;
using lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lexer
{
    public class LexerException<T> : Exception
    {

        public LexicalError Error { get; set; }

        public LexerException(LexicalError error)
        {
            Error = error;
        }

    }
}
