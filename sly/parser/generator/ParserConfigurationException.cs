using System;

namespace sly.parser.generator
{
    public class ParserConfigurationException : Exception
    {
        public ParserConfigurationException() : base("unable to configure parser")
        {
        }

        public ParserConfigurationException(string message) : base(message)
        {
        }
    }
}