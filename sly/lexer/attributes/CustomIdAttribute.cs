namespace sly.lexer
{
    public class CustomIdAttribute : LexemeAttribute
    {
        public CustomIdAttribute(string startPattern, string endPattern) : base(GenericToken.Identifier, IdentifierType.Custom,startPattern,endPattern)
        {
            
        } 
    }
}