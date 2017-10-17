using System;

namespace sly.pratt
{

    public class PrattParseException : Exception
    {
        public PrattParseException(String message) : base(message)
        {
        }
    }

}