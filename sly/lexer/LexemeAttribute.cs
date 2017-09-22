using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false,Inherited =true)]
    public class LexemeAttribute : Attribute
    {

        public string Pattern { get; set; }

        public bool IsSkippable { get; set; }

        public bool IsLineEnding { get; set; }

        public LexemeAttribute(string pattern, bool isSkippable = false, bool isLineEnding = false)        {
            Pattern = pattern;
            IsSkippable = isSkippable;
            IsLineEnding = isLineEnding;
        }
    }
}
