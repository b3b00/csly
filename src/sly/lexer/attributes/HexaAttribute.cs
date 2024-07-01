namespace sly.lexer;

public class HexaAttribute : LexemeAttribute
{
        
        
    public HexaAttribute(string hexaPrefix = "0x", int channel = Channels.Main) : base(GenericToken.Hexa,
        channel:channel, parameters:hexaPrefix)
    {
    }
}