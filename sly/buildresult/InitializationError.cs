using System;
using System.Collections.Generic;
using System.Text;

namespace sly.buildresult
{
    public class InitializationError 
    {
        public ErrorLevel Level { get; set; }

        public string Message { get; set; }

        public InitializationError(ErrorLevel level, string message) 
        {
            Message = message;
            Level = level;
        }

    }
}
