using System;

namespace sly.lexer
{
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IntAttribute : LexemeAttribute
    {
        public IntAttribute() : base(GenericToken.Int)
        {
            
        }
    }
}