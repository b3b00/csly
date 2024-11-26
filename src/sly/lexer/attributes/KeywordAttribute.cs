using System;

namespace sly.lexer
{
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class KeywordAttribute : LexemeAttribute
    {
        public KeywordAttribute(string keyword, int channel = Channels.Main) : base(GenericToken.KeyWord, keyword)
        {
            
        }
    }
}