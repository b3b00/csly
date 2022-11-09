using System;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ParserRootAttribute : Attribute
    {
        
        public string RootRule { get; set; }

        public ParserRootAttribute(string rootRule)
        {
            RootRule = rootRule;
        }

        
    }
}