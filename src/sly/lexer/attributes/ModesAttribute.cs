using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class ModesAttribute : Attribute
    {
        
        public IList<string> Modes { get; }

        public ModesAttribute(params string[] modes)
        {
            Modes = modes.ToList();
        }
        
    }
}