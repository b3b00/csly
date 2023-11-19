namespace sly.lexer
{
    public enum DateFormat
    {
        YYYYMMDD,
        DDMMYYYY
    }
    
    public class DateAttribute : LexemeAttribute
    {
        public DateAttribute(DateFormat format = DateFormat.DDMMYYYY, char separator = '-', int channel = Channels.Main) : base(GenericToken.Date,
            channel:channel, parameters:new[]{format.ToString(),separator.ToString()})
        {
        }
    }
}