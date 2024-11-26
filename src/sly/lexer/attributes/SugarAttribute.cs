using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class SugarAttribute : LexemeAttribute
    {
        public SugarAttribute(string token, int channel) : base(GenericToken.SugarToken, channel, token)
        {
            
        }
        
        public SugarAttribute(string token) : base(GenericToken.SugarToken, Channels.Main, token)
        {
            
        }
    }
}