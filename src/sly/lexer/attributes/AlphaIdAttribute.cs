using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AlphaIdAttribute : LexemeAttribute
    {
        public AlphaIdAttribute() : base(GenericToken.Identifier,IdentifierType.Alpha)
        {
            
        } 
    }
}