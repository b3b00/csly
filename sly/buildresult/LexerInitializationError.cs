using System;
using System.Collections.Generic;
using System.Text;

namespace sly.buildresult
{
    public class LexerInitializationError : InitializationError
    {

        public LexerInitializationError(ErrorLevel level, string message) : base(level, message)
        {
        }
    }
}
