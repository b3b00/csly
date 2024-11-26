using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class UpToAttribute : LexemeAttribute
    {
        public UpToAttribute(params string[] exceptions) : base(GenericToken.UpTo, exceptions)
        {
        }
    }
}