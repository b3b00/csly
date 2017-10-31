using System;
using System.Collections.Generic;
using System.Text;

namespace sly.buildresult
{
    public class ParserInitializationError : InitializationError
    {

        public ParserInitializationError(ErrorLevel level, string message) : base(level, message)
        {
        }
}
}
