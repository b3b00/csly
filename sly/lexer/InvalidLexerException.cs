using System;

namespace sly.lexer
{
    public class InvalidLexerException : Exception
    {
        public InvalidLexerException(string message) : base(message)
        {
        }
    }
}