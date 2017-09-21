using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false,Inherited =true)]
    public class LexemeAttribute : Attribute
    {

        public string Pattern { get; set; }

        public bool IsSkippable { get; set; }

        public bool IsEnding { get; set; }

        public LexemeAttribute(string pattern, bool isSkippable = false, bool isEnding = false)        {
            Pattern = pattern;
            IsSkippable = isSkippable;
            IsEnding = isEnding;
        }
    }
}
