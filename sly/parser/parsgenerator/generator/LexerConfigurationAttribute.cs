using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class LexerConfigurationAttribute : Attribute
    {
        
        public LexerConfigurationAttribute() {
            
        }
    }
}
