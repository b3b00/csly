using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CustomIdAttribute : LexemeAttribute
    {
        public CustomIdAttribute(string startPattern, string endPattern) : base(GenericToken.Identifier, IdentifierType.Custom,startPattern,endPattern)
        {
            
        } 
    }
}