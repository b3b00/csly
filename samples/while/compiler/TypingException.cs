using System;

namespace csly.whileLang.compiler
{
    public class TypingException : Exception
    {
        public TypingException(string message) : base(message)
        {
        }
    }
}