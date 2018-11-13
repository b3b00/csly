using System;

namespace sly.lexer
{
    public class LexerException : Exception
    {
        public LexerException(LexicalError error)
        {
            Error = error;
        }

        public LexicalError Error { get; set; }
    }
}