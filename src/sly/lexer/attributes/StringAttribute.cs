using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class StringAttribute : LexemeAttribute
    {
        public StringAttribute(string delimiterChar = "\"", string escapeChar = "\\", bool doEscape = true, int channel = Channels.Main) : base(GenericToken.String, channel, delimiterChar, escapeChar, doEscape.ToString())
        {   
        } 
    }
}