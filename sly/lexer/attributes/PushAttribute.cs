using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PushAttribute : Attribute
    {
        public string TargetMode { get; }

        public PushAttribute(string mode)
        {
            TargetMode = mode;
        }
    }
}