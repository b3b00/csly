using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PopAttribute : Attribute
    {
        public string TargetMode { get; }

        public PopAttribute(string mode = null)
        {
            TargetMode = mode;
        }
    }
}