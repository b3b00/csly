namespace sly.lexer
{
    public class DoubleAttribute : LexemeAttribute
    {
        public DoubleAttribute(string decimalDelimiter = ".", int channel = Channels.Main) : base(GenericToken.Double,
            channel:channel, parameters:decimalDelimiter)
        {
        }
    }
}