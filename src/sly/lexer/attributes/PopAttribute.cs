using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PopAttribute : Attribute
    {
        public PopAttribute()
        {
        }
    }
}